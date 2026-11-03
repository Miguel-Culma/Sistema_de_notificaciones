using PruebaTecnica_MonoLegal.BackgroundServices;
using Xunit;

namespace PruebaTecnica_MonoLegal.Tests.BackgroundServices
{
    public class ReminderSignalTests
    {
        // Se usa cuando esperamos que el programa NO despierte: si pasan 200 ms
        // y sigue esperando, entendemos que el timbre no sonó.
        private static readonly TimeSpan ShortWait = TimeSpan.FromMilliseconds(200);

        // Se usa cuando esperamos que el programa SÍ despierte. Normalmente
        // despierta casi al instante; los 2 segundos son solo un margen de sobra
        // por si la computadora va lenta.
        private static readonly TimeSpan LongWait = TimeSpan.FromSeconds(2);

        // CRONÓMETRO PARA LAS PRUEBAS
        // Pregunta: "¿la tarea terminó antes de que se acabara el tiempo?"
        //   - true  = sí, despertó a tiempo.
        //   - false = no, pasó el tiempo y sigue esperando.
        // Es necesario porque una espera puede durar para siempre, y no queremos
        // que la prueba se quede colgada.
        private static async Task<bool> CompletesWithinAsync(Task task, TimeSpan timeout)
        {
            // Es una carrera entre dos corredores: la tarea y un reloj.
            // Task.WhenAny devuelve al que llegue primero.
            var finished = await Task.WhenAny(task, Task.Delay(timeout));

            // Si ganó la tarea, terminó a tiempo. Si ganó el reloj, sigue esperando.
            return finished == task;
        }

        // ---------------------------------------------------------------
        // PRUEBA 1: Envia notificación antes de ponerse a esperar.
        // Lo esperado: el programa no tiene que esperar nada, ve que el timbre
        // ya sonó y sigue de inmediato.
        // ---------------------------------------------------------------
        [Fact]
        public async Task WaitAsync_AfterNotify_CompletesImmediately()
        {
            var signal = new ReminderSignal();
            signal.Notify();

            var wait = signal.WaitAsync(CancellationToken.None);

            Assert.True(await CompletesWithinAsync(wait, LongWait));
        }

        // ---------------------------------------------------------------
        // PRUEBA 2: No se envian notificaciones.
        // Lo esperado: el programa sigue esperando. No debe despertar solo
        // ---------------------------------------------------------------
        [Fact]
        public async Task WaitAsync_WithoutNotify_DoesNotComplete()
        {
            var signal = new ReminderSignal();

            var wait = signal.WaitAsync(CancellationToken.None);

            Assert.False(await CompletesWithinAsync(wait, ShortWait));
        }

        // ---------------------------------------------------------------
        // PRUEBA 3: el programa ya está esperando y llega una notificación.
        // Lo esperado: el programa despierta.
        // ---------------------------------------------------------------
        [Fact]
        public async Task WaitAsync_PendingThenNotify_Completes()
        {
            var signal = new ReminderSignal();

            var wait = signal.WaitAsync(CancellationToken.None);

            signal.Notify();

            Assert.True(await CompletesWithinAsync(wait, LongWait));
        }

        // ---------------------------------------------------------------
        // PRUEBA 4: apagan la aplicación mientras el programa espera.
        // ---------------------------------------------------------------
        [Fact]
        public async Task WaitAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            var signal = new ReminderSignal();

            // "cts" es como un botón de CANCELAR que podemos apretar a mano.
            using var cts = new CancellationTokenSource();

            // El programa se pone a esperar, conectado a ese botón.
            var wait = signal.WaitAsync(cts.Token);

            // Apretamos el botón: es como apagar la aplicación.
            cts.Cancel();

            // La espera debe terminar con un error de "cancelado".
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);
        }
    }
}