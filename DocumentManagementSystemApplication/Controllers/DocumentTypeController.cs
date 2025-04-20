using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentTypeController : ControllerBase
    {
        private readonly IDocumentTypeService _documentTypeService;

        public DocumentTypeController(IDocumentTypeService documentTypeService)
        {
            _documentTypeService = documentTypeService;
        }
        
        [HttpPost("create-document-type")]
        public async Task<ResponseDto> CreateDocumentType([FromBody] DocumentTypeDto documentTypeDto)
        {
            return await _documentTypeService.AddDocumentTypeAsync(documentTypeDto);
        }
        
        [HttpPost("update-document-type")]
        public async Task<ResponseDto> UpdateDocumentType([FromBody] DocumentTypeDto documentTypeDto)
        {
            return await _documentTypeService.UpdateDocumentTypeAsync(documentTypeDto);
        }
        
        [HttpGet("view-all-document-type")]
        public async Task<ResponseDto> ViewAllDocumentType([FromQuery] string? documentTypeName, [FromQuery] int page = 1,[FromQuery] int limit = 10)
        {
            return await _documentTypeService.GetAllDocumentTypeAsync(documentTypeName, page, limit);
        }
        
        [HttpPost("delete-document-type")]
        public async Task<ResponseDto> DeleteDocumentType([FromQuery] Guid documentTypeId)
        {
            return await _documentTypeService.UpdateDocumentTypeActiveOrDeleteAsync(documentTypeId);
        }
        
        [HttpGet("view-document-type-details")]
        public async Task<ResponseDto> ViewDocumentTypeDetails([FromQuery] Guid documentTypeId)
        {
            return await _documentTypeService.GetDocumentTypeDetails(documentTypeId);
        }

        [HttpGet("view-document-type-name-by-workflow-id")]
        public async Task<ResponseDto> ViewDocumentTypeNameByWorkflowId([FromQuery] Guid workflowId)
        {
            return await _documentTypeService.GetAllDocumentTypeNameByWorkflowIdAsync(workflowId);
        }
    }
}
