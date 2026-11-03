using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using PruebaTecnica_MonoLegal.Services.Interfaces;

namespace PruebaTecnica_MonoLegal.BackgroundServices
{
    public class ReminderBackgroundService : BackgroundService
    {
        private static readonly TimeSpan ErrorRetryDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan MinWait = TimeSpan.FromSeconds(1);

        // Cada estado espera (intervalo base × factor) contado desde que
        // LA FACTURA entró a ese estado.
        private static readonly Dictionary<string, double> StateIntervalFactors = new()
        {
            ["primerrecordatorio"] = 1.0,
            ["segundorecordatorio"] = 0.5
        };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly ReminderSignal _signal;
        // Solo para pruebas: permite simular desde cuándo está activo el servicio.
        internal DateTime EnabledSince
        {
            get => _enabledSince;
            set => _enabledSince = value;
        }

        // Evita contar como "ciclo cumplido" el tiempo con el servicio apagado.
        private DateTime _enabledSince = DateTime.UtcNow;

        // Última vez (UTC) que ESTE servicio movió cada factura.
        // No depende de zonas horarias ni de lo que devuelva la base de datos.
        private readonly Dictionary<string, DateTime> _lastMoved = new();

        public ReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReminderBackgroundService> logger,
            ReminderSignal signal)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _signal = signal;
        }

        // Convierte cualquier DateTime a UTC de forma correcta.
        private static DateTime ToUtc(DateTime d) => d.Kind switch
        {
            DateTimeKind.Local => d.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(d, DateTimeKind.Utc),
            _ => d
        };

        // ───────────── 1. Bucle principal ─────────────

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio automático de recordatorios iniciado.");
            var wasEnabled = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var settings = await scope.ServiceProvider
                        .GetRequiredService<IReminderSettingsRepository>()
                        .GetOrCreateAsync();

                    TimeSpan? waitTime = null;

                    if (!settings.enabled)
                    {
                        wasEnabled = false;
                        _logger.LogInformation("Recordatorios desactivados: ciclo detenido.");
                    }
                    else
                    {
                        if (!wasEnabled)
                        {
                            wasEnabled = true;
                            _enabledSince = DateTime.UtcNow;
                            _lastMoved.Clear();
                            _logger.LogInformation(
                                "Recordatorios activos. Intervalo base: {Interval} min.",
                                settings.intervalMinutes);
                        }

                        var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                        waitTime = await ProcessDueInvoicesAsync(
                            invoiceService, settings.intervalMinutes, stoppingToken);
                    }

                    await WaitForNextEventAsync(waitTime, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante el ciclo automático de recordatorios.");
                    try { await Task.Delay(ErrorRetryDelay, stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }

            _logger.LogInformation("Servicio automático de recordatorios detenido.");
        }

        // ───────────── 2. Consultar y procesar facturas ─────────────

        // Procesa las facturas cuyo ciclo YA se cumplió en su estado actual
        // y devuelve cuánto esperar hasta la próxima que toque.
        internal async Task<TimeSpan> ProcessDueInvoicesAsync(
            IInvoiceService invoiceService,
            int baseIntervalMinutes,
            CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            TimeSpan? nextWait = null;
            var processedAny = false;

            // Se consultan todos los estados ANTES de procesar, para que una
            // factura que pasa de primero a segundo no se procese dos veces
            // en la misma vuelta.
            var snapshot = new List<(string State, TimeSpan Interval, Invoice Invoice)>();

            foreach (var (state, factor) in StateIntervalFactors)
            {
                var stateInterval = TimeSpan.FromMinutes(Math.Max(1, baseIntervalMinutes) * factor);

                try
                {
                    var found = await invoiceService.GetInvoicesByStateAsync(state);
                    foreach (var invoice in found)
                        snapshot.Add((state, stateInterval, invoice));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudieron consultar las facturas en estado {State}.", state);
                }
            }

            // Limpia del diccionario las facturas que ya no están en seguimiento.
            var activeIds = snapshot.Select(s => s.Invoice._id.ToString()).ToHashSet();
            foreach (var key in _lastMoved.Keys.Where(k => !activeIds.Contains(k)).ToList())
                _lastMoved.Remove(key);

            foreach (var (state, stateInterval, invoice) in snapshot)
            {
                if (stoppingToken.IsCancellationRequested)
                    return MinWait;

                var id = invoice._id.ToString();

                // Reloj PROPIO de la factura: desde que entró a este estado.
                var enteredAt = ToUtc(invoice.updatedAt);

                // Si el servicio la movió hace poco, ese momento manda sobre la BD.
                if (_lastMoved.TryGetValue(id, out var movedAt) && movedAt > enteredAt)
                    enteredAt = movedAt;

                // El tiempo con el servicio apagado no cuenta.
                if (enteredAt < _enabledSince)
                    enteredAt = _enabledSince;

                var remaining = enteredAt + stateInterval - now;

                _logger.LogInformation(
                    "Factura {Id} en {State}: updatedAt={Raw} (Kind={Kind}), enteredAt={Entered:o}, now={Now:o}, falta={Remaining}",
                    invoice._id, state, invoice.updatedAt, invoice.updatedAt.Kind,
                    enteredAt, now, remaining);

                if (remaining > TimeSpan.Zero)
                {
                    // Aún no cumple su ciclo completo en este estado.
                    if (nextWait is null || remaining < nextWait)
                        nextWait = remaining;
                    continue;
                }

                try
                {
                    // Avanza UN solo paso y actualiza updatedAt en el repositorio.
                    await invoiceService.ProcessReminderAsync(id);
                    _lastMoved[id] = DateTime.UtcNow;
                    processedAny = true;

                    _logger.LogInformation(
                        "Factura {InvoiceId} procesada desde {State}.", invoice._id, state);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo procesar la factura {InvoiceId}.", invoice._id);
                }
            }

            // Si se movió alguna factura, se recalcula pronto: la que pasó a
            // segundo ya tiene su reloj nuevo y hay que programar su espera.
            if (processedAny)
                return MinWait;

            // Sin nada pendiente: se vuelve a revisar tras el ciclo más corto
            // (por si entran facturas nuevas) o hasta la próxima que toque.
            var fallback = TimeSpan.FromMinutes(
                Math.Max(1, baseIntervalMinutes) * StateIntervalFactors.Values.Min());

            var wait = nextWait ?? fallback;
            return wait < MinWait ? MinWait : wait;
        }

        // ───────────── 3. Dormir hasta temporizador o señal ─────────────

        private async Task WaitForNextEventAsync(TimeSpan? waitTime, CancellationToken stoppingToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            var signalTask = _signal.WaitAsync(linkedCts.Token);
            var timerTask = Task.Delay(waitTime ?? Timeout.InfiniteTimeSpan, linkedCts.Token);

            _logger.LogInformation(
                "Durmiendo {Wait}.",
                waitTime.HasValue ? waitTime.Value.ToString(@"hh\:mm\:ss") : "hasta recibir una señal");

            var finished = await Task.WhenAny(signalTask, timerTask);

            linkedCts.Cancel();
            stoppingToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Despertó por: {Reason}.",
                finished == signalTask ? "cambio de configuración" : "temporizador");
        }
    }
}