using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;
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
        private readonly IFileService _fileService;

        public DocumentController(IDocumentService documentService, IFileService fileService)
        {
            _documentService = documentService;
            _fileService = fileService;
        }
        
        [AllowAnonymous]
        [HttpGet("view-test")]
        public  IActionResult GetViewTest([FromQuery]string pathFile,[FromQuery]string output)
        {
              _fileService.InsertTextAsImageToPdf("/home/wiramin/Downloads/Mau to trinh(1) (2).pdf","/home/wiramin/Data/project/Capstone_2025/Document_Management_System/DocumentManagementSystemApplication/data/storage/output","Số:09/2025/TT-TNABC",89+20,659+20,209+10,679+15);
              return Ok();
        }

        [AllowAnonymous]
        [HttpPost("create-convert-doc-to-pdf")]
        public async Task<IActionResult> GetViewTest(IFormFile file)
        {
            return await _fileService.ConvertDocToPdf(file);
        }


        [HttpPost("create-upload-document")]
        public async Task<ResponseDto> UploadDocument(IFormFile file)
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
        public async Task<IActionResult> DownloadDocumentByName([FromRoute] Guid documentId,
            [FromQuery] string? version, [FromQuery] bool isArchive)
        {
            if (!isArchive)
                return await _documentService.GetDocumentById(documentId, version);
            else
                return await _documentService.GetArchiveDocumentById(documentId, version);
        }

        [HttpGet("view-file-by-name")]
        public async Task<IActionResult> DownloadDocumentByFileName([FromQuery] string documentName)
        {
            var id = User.FindFirst("userid")?.Value;
            return await _documentService.GetDocumentByFileName(documentName, Guid.Parse(id));
        }

        [HttpPost("update-confirm-task-with-document")]
        public async Task<ResponseDto> UpdateConfirmTaskWithDocument([FromQuery] Guid documentId)
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
        public async Task<ResponseDto> ViewAllDocumentsMobile([FromQuery] Guid? workFlowId,
            [FromQuery] Guid documentTypeId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllDocumentsMobile(workFlowId, documentTypeId, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-detail-documents-mobile")]
        public async Task<ResponseDto> ViewDetailDocumentsMobile([FromQuery] Guid documentId,
            [FromQuery] Guid workFlowId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetDocumentDetailByIdMobile(documentId, Guid.Parse(id), workFlowId);
            return result;
        }

        [HttpGet("view-all-documents-by-document-type-mobile")]
        public async Task<ResponseDto> ViewAllDocumentsByTypeMobile([FromQuery] Guid documentTypeId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllDocumentsByTypeMobile(documentTypeId, Guid.Parse(id));
            return result;
        }

        [HttpGet("update-clear-cache-document-mobile")]
        public async Task<ResponseDto> UpdateClearCacheDocumentMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.ClearCacheDocumentMobile(Guid.Parse(id));
            return result;
        }

        [HttpGet("view-document-by-name")]
        public async Task<ResponseDto> GetDocumentByNameMobile([FromQuery] string documentName)
        {
            var id = User.FindFirst("userid")?.Value;

            var result = await _documentService.GetDocumentByNameMobile(documentName, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-detail-document")]
        public async Task<ResponseDto> ViewDetailDocument([FromQuery] Guid documentId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetDocumentDetailById(documentId, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-my-self-document")]
        public async Task<ResponseDto> ViewMySelfDocument([FromQuery] string? searchText, [FromQuery] int page = 1,
            int pageSize = 10)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetMySelfDocument(Guid.Parse(id), searchText, page, pageSize);
            return result;
        }

        [HttpGet("view-process-document-detail")]
        public async Task<ResponseDto> ViewProcessDocumentDetail([FromQuery] Guid? documentId)
        {
            var result = await _documentService.ShowProcessDocumentDetail(documentId);
            return result;
        }
        // [HttpPost("create-document-by-template")]
        // public async Task<ResponseDto> CreateDocumentByTemplate([FromBody] DocumentUploadDto documentUploadDto)
        // {
        //     var id = User.FindFirst("userid")?.Value;
        //     var result = await _documentService.CreateDocumentByTemplate(documentUploadDto, Guid.Parse(id));
        //     return result;
        // }
    
}
}
