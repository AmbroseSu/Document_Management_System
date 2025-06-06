using DataAccess.DTO;
using DataAccess.DTO.Request;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class ArchiveDocumentController : ControllerBase
    {
        private readonly IArchiveDocumentService _archiveDocumentService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ArchiveDocumentController(IArchiveDocumentService archiveDocumentService, IEmailService emailService, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _archiveDocumentService = archiveDocumentService;
            _emailService = emailService;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        /*[HttpPost("create-upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] IFormFile file)
        {
            var signatureInfo = _archiveDocumentService.ExtractSignatures(file);
            return Ok(new { Signatures = signatureInfo });
        }*/
        
        
        [HttpPost("create-send-email")]
        //[AuthorizeResource("[Archivedocument] Create Send Email")]
        public async Task<ResponseDto> SendEmail([FromBody] EmailRequest emailRequest)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _emailService.SendEmailWithOAuth2(emailRequest, Guid.Parse(userId));
            return result;
        }
        
        // [HttpPost("create-send")]
        // [AuthorizeResource("[ArchiveDocument] Create Send Notification")]
        // public async Task<IActionResult> SendNotification([FromBody] string message)
        // {
        //     await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        //     return Ok(new { message = "Notification sent" });
        // }
        
        [HttpPost("view-all-documents")]
        //[AuthorizeResource("[Archivedocument] View All Documents")]
        public async Task<ResponseDto> GetAllDocuments([FromBody]GetAllArchiveRequestDto getAllArchiveRequestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            
            //search name, scope, created date, status
            var id = User.FindFirst("userid")?.Value;
            var documents = await _archiveDocumentService.GetAllArchiveDocuments(getAllArchiveRequestDto,Guid.Parse(id),page,pageSize);
            return documents;
        }

        [HttpGet("view-archive-document-detail")]
        //[AuthorizeResource("[Archivedocument] View Archive Document Detail")]
        public async Task<ResponseDto> GetArchiveDocumentDetail([FromQuery] Guid documentId)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.GetArchiveDocumentDetail(documentId, Guid.Parse(userId));
            return result;
        }
        
        [HttpGet("view-all-templates")]
        //[AuthorizeResource("[Archivedocument] View All Templates")]
        public async Task<ResponseDto> GetAllTemplates([FromQuery]string? documentName, [FromQuery]string? name,[FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var templates = await _archiveDocumentService.GetAllArchiveTemplates(documentName,name,page,pageSize);
            return templates;
        }
        
        [HttpPost("create-template")]
        //[AuthorizeResource("[Archivedocument] Create Template")]
        public async Task<ResponseDto> CreateTemplate([FromForm] ArchiveDocumentRequest archiveDocumentRequest)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.CreateArchiveTemplate(archiveDocumentRequest, Guid.Parse(userId));
            return result;
        }
        
        [HttpDelete("delete-template")]
        public async Task<ResponseDto> DeleteTemplate([FromQuery] Guid templateId)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.DeleteArchiveTemplate(templateId, Guid.Parse(userId));
            return result;
        }
        
        [HttpGet("view-download-template")]
        //[AuthorizeResource("[Archivedocument] View Download Template")]
        public async Task<IActionResult> DownloadTemplate([FromQuery] string templateId,[FromQuery]bool? isPdf = false)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.DownloadTemplate(templateId, Guid.Parse(userId),isPdf);
            return result;
        }
        
        // [HttpPost("create-template")]
        
        
        /*[HttpPost("create-send-to-user")]
        [AuthorizeResource("[Archivedocument] Create Send To User")]
        public async Task<IActionResult> SendToUser([FromQuery] string userId, [FromQuery] string message)
        {
            // Gửi đến userId cụ thể
            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveMessage", message);

            return Ok(new { message = $"Notification sent to user {userId}" });
        }*/
        
        // [HttpPost("create-send-test")]
        // public async Task SendPushNotificationMobileAsync([FromQuery] string deviceToken)
        // {
        //     var id = User.FindFirst("userid")?.Value;
        //     var noti = _notificationService.TestNotification(Guid.Parse(id));
        //     await _notificationService.SendPushNotificationMobileAsync(deviceToken, noti);
        //     await _hubContext.Clients.All.SendAsync("ReceiveMessage", noti);
        // }
        
        [HttpPost("create-withdraw-document")]
        //[AuthorizeResource("[Archivedocument] Create Withdraw Document")]
        public async Task<ResponseDto> WithdrawDocument([FromQuery]Guid archiveDocumentId,[FromBody]DocumentPreInfo documentPreInfo)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.WithdrawArchiveDocument(archiveDocumentId,documentPreInfo, Guid.Parse(userId));
            return result;
        }
        
        [HttpPost("create-replace-document")]
        public async Task<ResponseDto> ReplaceDocument([FromQuery]Guid archiveDocumentId,[FromBody]DocumentPreInfo documentPreInfo)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.ReplaceArchiveDocument(archiveDocumentId,documentPreInfo, Guid.Parse(userId));
            return result;
        }

        [HttpPost("update-withdraw-document-by-id")]
        public async Task<ResponseDto> WithdrawDocumentById([FromQuery] Guid archiveDocumentId)
        {
            var userId = User.FindFirst("userid")?.Value;
            var result = await _archiveDocumentService.WithdrawArchiveDocument(Guid.Parse(userId),archiveDocumentId);
            return result;
        }
    }
}
