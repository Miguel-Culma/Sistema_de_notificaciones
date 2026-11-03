using Microsoft.AspNetCore.Mvc;
using PruebaTecnica_MonoLegal.BackgroundServices;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace PruebaTecnica_MonoLegal.Controllers
{
    [Authorize()]
    [ApiController]
    [Route("api/[controller]")]
    public class ReminderSettingsController : ControllerBase
    {
        private readonly IReminderSettingsRepository _settingsRepository;
        private readonly ReminderSignal _reminderSignal;

        public ReminderSettingsController(
            IReminderSettingsRepository settingsRepository,
            ReminderSignal reminderSignal)
        {
            _settingsRepository = settingsRepository;
            _reminderSignal = reminderSignal;
        }

        // Consultar configuración actual
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _settingsRepository.GetOrCreateAsync();

            return Ok(settings);
        }

        // Cambiar intervalo en minutos
        [Authorize(Roles = "Admin")]
        [HttpPut("interval")]
        public async Task<IActionResult> UpdateInterval(
            [FromBody] UpdateIntervalRequest request)
        {
            if (request.IntervalMinutes < 1)
            {
                return BadRequest(new
                {
                    message = "El intervalo debe ser de al menos 1 minuto."
                });
            }

            var settings = await _settingsRepository.UpdateIntervalAsync(
                request.IntervalMinutes);

            // Avisa al servicio de fondo para que recalcule sus tiempos ya.
            _reminderSignal.Notify();

            return Ok(new
            {
                message = "Intervalo actualizado correctamente.",
                intervalMinutes = settings.intervalMinutes,
                updatedAt = settings.updatedAt
            });
        }

        // Activar o desactivar automatización
        [Authorize(Roles = "Admin")]
        [HttpPut("enabled")]
        public async Task<IActionResult> UpdateEnabled(
            [FromBody] UpdateEnabledRequest request)
        {
            var settings = await _settingsRepository.UpdateEnabledAsync(
                request.Enabled);

            // Avisa al servicio de fondo para que se active/desactive ya.
            _reminderSignal.Notify();

            return Ok(new
            {
                message = settings.enabled
                    ? "Recordatorios automáticos activados."
                    : "Recordatorios automáticos desactivados.",
                enabled = settings.enabled,
                updatedAt = settings.updatedAt
            });
        }
    }

    public class UpdateIntervalRequest
    {
        public int IntervalMinutes { get; set; }
    }

    public class UpdateEnabledRequest
    {
        public bool Enabled { get; set; }
    }
}