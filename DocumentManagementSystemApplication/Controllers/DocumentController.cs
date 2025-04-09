using BusinessObject;
using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("view-metadata-document")]
        public async Task<string> ReadMetadataDocument([FromForm] IFormFile file)
        {
            await _documentService.UploadDocument(file, Guid.Empty);
            return "ok";

        }
        
        [HttpPost("create-upload-document/{userId}")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromRoute] Guid userId)
        {
            var result = await _documentService.UploadDocument(file, userId);
            return Ok(result);
        }
    }
}
