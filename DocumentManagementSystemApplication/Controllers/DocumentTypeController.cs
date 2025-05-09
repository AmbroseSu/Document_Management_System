using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentTypeController : ControllerBase
    {
        private readonly IDocumentTypeService _documentTypeService;

        public DocumentTypeController(IDocumentTypeService documentTypeService)
        {
            _documentTypeService = documentTypeService;
        }
        
        [HttpPost("create-document-type")]
        //[AuthorizeResource("[Documenttype] Create Document Type")]
        public async Task<ResponseDto> CreateDocumentType([FromBody] DocumentTypeDto documentTypeDto)
        {
            return await _documentTypeService.AddDocumentTypeAsync(documentTypeDto);
        }
        
        [HttpPost("update-document-type")]
        //[AuthorizeResource("[Documenttype] Update Document Type")]
        public async Task<ResponseDto> UpdateDocumentType([FromBody] DocumentTypeDto documentTypeDto)
        {
            return await _documentTypeService.UpdateDocumentTypeAsync(documentTypeDto);
        }
        
        [HttpGet("view-all-document-type")]
        //[AuthorizeResource("[Documenttype] View All Document Type")]
        public async Task<ResponseDto> ViewAllDocumentType([FromQuery] string? documentTypeName, [FromQuery] int page = 1,[FromQuery] int limit = 10)
        {
            return await _documentTypeService.GetAllDocumentTypeAsync(documentTypeName, page, limit);
        }
        
        [HttpPost("delete-document-type")]
        //[AuthorizeResource("[Documenttype] Delete Document Type")]
        public async Task<ResponseDto> DeleteDocumentType([FromQuery] Guid documentTypeId)
        {
            return await _documentTypeService.UpdateDocumentTypeActiveOrDeleteAsync(documentTypeId);
        }
        
        [HttpGet("view-document-type-details")]
        //[AuthorizeResource("[Documenttype] View Document Type Details")]
        public async Task<ResponseDto> ViewDocumentTypeDetails([FromQuery] Guid documentTypeId)
        {
            return await _documentTypeService.GetDocumentTypeDetails(documentTypeId);
        }

        [HttpGet("view-document-type-name-by-workflow-id")]
        //[AuthorizeResource("[Documenttype] View Document Type Name By Workflow Id")]
        public async Task<ResponseDto> ViewDocumentTypeNameByWorkflowId([FromQuery] Guid workflowId)
        {
            return await _documentTypeService.GetAllDocumentTypeNameByWorkflowIdAsync(workflowId);
        }
    }
}
