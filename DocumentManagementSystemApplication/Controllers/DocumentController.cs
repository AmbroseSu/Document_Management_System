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
        
        [HttpPost("create-upload-document")]
        public async Task<ResponseDto> UploadDocument([FromForm] IFormFile file)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UploadDocument(file, id);
            return result;
        }
        
        [HttpPost("create-incoming-document")]
        public async Task<ResponseDto> CreateIncomingDocument([FromBody] DocumentUploadDto documentUploadDto)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.CreateIncomingDoc(documentUploadDto, Guid.Parse(id));
            return result;
        }
        
        
        [HttpGet("view-file/{documentId}")]
        public async Task<IActionResult> DownloadDocumentByName([FromRoute] Guid documentId,[FromQuery] string? version,[FromQuery] bool isArchive)
        {
            if(!isArchive)
                return  await _documentService.GetDocumentById(documentId,version);
            else
                return  await _documentService.GetArchiveDocumentById(documentId, version);
        }
        
        [HttpPost("update-confirm-task-with-document")]
        public async Task<ResponseDto> UpdateConfirmTaskWithDocument([FromQuery]Guid documentId)
        {
            // var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UpdateConfirmTaskWithDocument(documentId);
            return result;
        }

        [HttpGet("view-all-type-documents-by-workflow-mobile")]
        public async Task<ResponseDto> ViewAllTypeDocumentsByWorkFlowMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllTypeDocumentsMobile(Guid.Parse(id));
            return result;
        }
        
        [HttpGet("view-all-type-documents-mobile")]
        public async Task<ResponseDto> ViewAllTypeDocumentsMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllTypeDocMobile(Guid.Parse(id));
            return result;
        }
        
        [HttpGet("view-all-documents-mobile")]
        public async Task<ResponseDto> ViewAllDocumentsMobile([FromQuery] Guid? workFlowId,[FromQuery] Guid documentTypeId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllDocumentsMobile(workFlowId, documentTypeId, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-detail-documents-mobile")]
        public async Task<ResponseDto> ViewAllDocumentsMobile([FromQuery] Guid documentId,[FromQuery] Guid workFlowId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetDocumentDetailById(documentId, Guid.Parse(id),workFlowId);
            return result;
        }
        
        [HttpGet("update-clear-cache-document-mobile")]
        public async Task<ResponseDto> UpdateClearCacheDocumentMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.ClearCacheDocumentMobile(Guid.Parse(id));
            return result;
        }
    }
}
