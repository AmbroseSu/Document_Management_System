using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        
        [HttpGet("view-notifications-by-user-id")]
        public async Task<IActionResult> GetNotificationsByUserId([FromQuery] string userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var response = await _notificationService.GetNotificationsByUserIdAsync(userId, page, limit);
            return Ok(response);
        }

        [HttpPost("update-mark-notification-as-read")]
        public async Task MarkNotificationAsRead([FromQuery] Guid notificationId)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId);
        }
    }
}
