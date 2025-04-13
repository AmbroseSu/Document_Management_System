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
            var id = User.FindFirst("userid")?.Value;
            return id;
        }

        
        [HttpPost("create-upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UploadDocument(file, id);
            return Ok(result);
        }
    }
}
