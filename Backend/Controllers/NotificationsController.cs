
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;

namespace PruebaTecnica_MonoLegal.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications =
                await _notificationRepository.GetAllAsync();

            return Ok(notifications);
        }
    }
}
