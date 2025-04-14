using BusinessObject;
using DataAccess.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        
        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // [HttpPost("view-metadata-document")]
        // public async Task<string> ReadMetadataDocument([FromForm] IFormFile file)
        // {
        //     await _documentService.UploadDocument(file, Guid.Empty);
        //     return "ok";
        //
        // }
        
        
        [HttpGet("view-hello-document")]
        public async Task<string?> ViewHelloDocument()
        {
            var id = User.FindFirst("userid")!.Value;
            return id;
        }

        
        [HttpPost("create-upload-document")]
        public async Task<ResponseDto> UploadDocument([FromForm] IFormFile file)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UploadDocument(file, id);
            return result;
        }
        
        [HttpGet("view-dowload-document-by-id")]
        public async Task<IActionResult> DownloadDocumentById([FromQuery] Guid documentId)
        {
            var result = await _documentService.GetDocumentById(documentId);
            return result;
        }
        
        [HttpGet("view-download-document-by-name")]
        public async Task<IActionResult> DownloadDocumentByName([FromQuery] string documentName)
        {
            var result = await _documentService.GetDocumentByName(documentName);
            return result;
        }
    }
}
