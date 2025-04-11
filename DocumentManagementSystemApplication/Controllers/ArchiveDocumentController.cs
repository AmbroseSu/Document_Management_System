using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Utilities;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchiveDocumentController : ControllerBase
    {
        private readonly IArchiveDocumentService _archiveDocumentService;
        private readonly IEmailService _emailService;

        public ArchiveDocumentController(IArchiveDocumentService archiveDocumentService, IEmailService emailService)
        {
            _archiveDocumentService = archiveDocumentService;
            _emailService = emailService;
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
    }
}
