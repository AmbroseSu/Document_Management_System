using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service;
using Service.SignalRHub;
using Service.Utilities;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchiveDocumentController : ControllerBase
    {
        private readonly IArchiveDocumentService _archiveDocumentService;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ArchiveDocumentController(IArchiveDocumentService archiveDocumentService, IEmailService emailService, IHubContext<NotificationHub> hubContext)
        {
            _archiveDocumentService = archiveDocumentService;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        /*[HttpPost("create-upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] IFormFile file)
        {
            var signatureInfo = _archiveDocumentService.ExtractSignatures(file);
            return Ok(new { Signatures = signatureInfo });
        }*/
        
        
        [HttpPost("create-send-email")]
        public async Task<ResponseDto> SendEmail([FromForm] EmailRequest emailRequest)
        {
            var result = await _emailService.SendEmailWithOAuth2(emailRequest);
            return result;
        }
        
        [HttpPost("create-send")]
        public async Task<IActionResult> SendNotification([FromBody] string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            return Ok(new { message = "Notification sent" });
        }
    }
}
