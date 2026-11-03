using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Bson;
using PruebaTecnica_MonoLegal.BackgroundServices;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Services.Interfaces;
using Xunit;

namespace PruebaTecnica_MonoLegal.Tests.BackgroundServices
{
    public class ReminderBackgroundServiceTests
    {
        private const string First = "primerrecordatorio";
        private const string Second = "segundorecordatorio";

        // Intervalo base de las pruebas: primer estado = 30 min, segundo = 15 min.
        private const int BaseInterval = 30;

        private readonly Mock<IInvoiceService> _invoiceService;
        private readonly ReminderBackgroundService _service;

        public ReminderBackgroundServiceTests()
        {
            _invoiceService = new Mock<IInvoiceService>(MockBehavior.Strict);

            // ProcessDueInvoicesAsync no usa el scope factory ni la señal,
            // así que basta con instancias vacías.
            var scopeFactory = new ServiceCollection()
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();

            _service = new ReminderBackgroundService(
                scopeFactory,
                NullLogger<ReminderBackgroundService>.Instance,
                new ReminderSignal());

            // El servicio "lleva activo" 3 horas: así el tiempo anterior
            // a su arranque no recorta el reloj de las facturas de prueba.
            _service.EnabledSince = DateTime.UtcNow.AddHours(-3);
        }

        // minutesInState = cuánto lleva la factura en su estado actual.
        private static Invoice CreateInvoice(string status, double minutesInState)
        {
            return new Invoice
            {
                _id = ObjectId.GenerateNewId(),
                invoiceNumber = "FAC-" + Guid.NewGuid().ToString("N")[..6],
                clientId = ObjectId.GenerateNewId(),
                amount = 100000,
                currency = "COP",
                createAt = DateTime.UtcNow,
                dueDate = DateTime.UtcNow.AddDays(10),
                status = status,
                updatedAt = DateTime.UtcNow.AddMinutes(-minutesInState)
            };
        }

        // El servicio consulta SIEMPRE ambos estados, así que se configuran los dos.
        private void SetupInvoices(Invoice[] firstState, Invoice[] secondState)
        {
            _invoiceService
                .Setup(s => s.GetInvoicesByStateAsync(First))
                .ReturnsAsync(firstState.ToList());
            _invoiceService
                .Setup(s => s.GetInvoicesByStateAsync(Second))
                .ReturnsAsync(secondState.ToList());
        }

        private void SetupProcessOk(Invoice invoice)
        {
            _invoiceService
                .Setup(s => s.ProcessReminderAsync(invoice._id.ToString()))
                .Returns(Task.CompletedTask);
        }

        private void VerifyProcessed(Invoice invoice, Times times)
        {
            _invoiceService.Verify(
                s => s.ProcessReminderAsync(invoice._id.ToString()),
                times);
        }

        // ---------------------------------------------------------------
        // Prueba 1: procesa las facturas vencidas de todos los estados
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_ProcessesDueInvoicesFromAllStates()
        {
            var a = CreateInvoice(First, minutesInState: 60);   // 60 > 30 → vencida
            var b = CreateInvoice(Second, minutesInState: 60);  // 60 > 15 → vencida

            SetupInvoices(new[] { a }, new[] { b });
            SetupProcessOk(a);
            SetupProcessOk(b);

            await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);

            VerifyProcessed(a, Times.Once());
            VerifyProcessed(b, Times.Once());
        }

        // ---------------------------------------------------------------
        // Prueba 2: una factura que aún no cumple su ciclo NO se procesa
        // (este es el bug de la jerarquía)
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_InvoiceNotDueYet_IsNotProcessed()
        {
            var justEntered = CreateInvoice(Second, minutesInState: 1); // 1 < 15

            SetupInvoices(Array.Empty<Invoice>(), new[] { justEntered });

            var wait = await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);

            VerifyProcessed(justEntered, Times.Never());

            // Debe esperar aproximadamente lo que le falta (~14 min), no un segundo.
            Assert.InRange(wait, TimeSpan.FromMinutes(13), TimeSpan.FromMinutes(15));
        }

        // ---------------------------------------------------------------
        // Prueba 3: cada estado usa su propio tiempo
        // (primero = intervalo completo, segundo = la mitad)
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_EachStateUsesItsOwnInterval()
        {
            var inFirst = CreateInvoice(First, minutesInState: 20);   // 20 < 30 → espera
            var inSecond = CreateInvoice(Second, minutesInState: 20); // 20 > 15 → vencida

            SetupInvoices(new[] { inFirst }, new[] { inSecond });
            SetupProcessOk(inSecond);

            await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);

            VerifyProcessed(inFirst, Times.Never());
            VerifyProcessed(inSecond, Times.Once());
        }

        // ---------------------------------------------------------------
        // Prueba 4: tras mover una factura, no se vuelve a procesar enseguida
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_AfterProcessing_InvoiceWaitsItsNextCycle()
        {
            var a = CreateInvoice(First, minutesInState: 60);

            // El mock sigue devolviendo la factura con updatedAt viejo, igual que
            // si la base de datos tardara en reflejar el cambio: el servicio debe
            // recordar por sí mismo que ya la movió.
            SetupInvoices(new[] { a }, Array.Empty<Invoice>());
            SetupProcessOk(a);

            await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);
            await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);

            VerifyProcessed(a, Times.Once());
        }

        // ---------------------------------------------------------------
        // Prueba 5: si una factura falla, las demás se siguen procesando
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_OneInvoiceFails_ContinuesWithTheRest()
        {
            var failing = CreateInvoice(First, minutesInState: 60);
            var healthy = CreateInvoice(First, minutesInState: 60);

            SetupInvoices(new[] { failing, healthy }, Array.Empty<Invoice>());

            _invoiceService
                .Setup(s => s.ProcessReminderAsync(failing._id.ToString()))
                .ThrowsAsync(new InvalidOperationException("falló"));
            SetupProcessOk(healthy);

            // No debe lanzar excepción hacia afuera
            await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);

            VerifyProcessed(failing, Times.Once());
            VerifyProcessed(healthy, Times.Once());
        }

        // ---------------------------------------------------------------
        // Prueba 6: sin facturas no se procesa nada
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_NoInvoices_DoesNotProcessAnything()
        {
            SetupInvoices(Array.Empty<Invoice>(), Array.Empty<Invoice>());

            var wait = await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, CancellationToken.None);

            _invoiceService.Verify(
                s => s.ProcessReminderAsync(It.IsAny<string>()),
                Times.Never());

            // Sin facturas, vuelve a revisar tras el ciclo más corto (15 min).
            Assert.Equal(TimeSpan.FromMinutes(15), wait);
        }

        // ---------------------------------------------------------------
        // Prueba 7: cuando se para el servicio no se procesa ninguna factura
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessDueInvoicesAsync_CancellationRequested_DoesNotProcessInvoices()
        {
            var a = CreateInvoice(First, minutesInState: 60);
            SetupInvoices(new[] { a }, Array.Empty<Invoice>());

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await _service.ProcessDueInvoicesAsync(
                _invoiceService.Object, BaseInterval, cts.Token);

            VerifyProcessed(a, Times.Never());
        }
    }
}