using System.Threading.Channels;

namespace PruebaTecnica_MonoLegal.BackgroundServices
{
    /// avisaa al servicio de recordatorios
    /// que la configuración cambió y debe despertar de inmediato.
    public class ReminderSignal
    {
        // Canal con capacidad 1: si llegan varios avisos seguidos se fusionan en uno,
        // porque al despertar el servicio vuelve a leer la configuración completa.
        private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropWrite
            });

        // Lo llama quien modifica la configuración (endpoint).
        public void Notify() => _channel.Writer.TryWrite(true);

        // Lo usa el servicio: espera hasta que llegue un aviso y lo consume.
        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            await _channel.Reader.WaitToReadAsync(cancellationToken);
            _channel.Reader.TryRead(out _);
        }
    }
}