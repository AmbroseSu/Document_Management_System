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

        public ArchiveDocumentController(IArchiveDocumentService archiveDocumentService)
        {
            _archiveDocumentService = archiveDocumentService;
        }

        /*[HttpPost("create-upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] IFormFile file)
        {
            var signatureInfo = _archiveDocumentService.ExtractSignatures(file);
            return Ok(new { Signatures = signatureInfo });
        }*/
    }
}
