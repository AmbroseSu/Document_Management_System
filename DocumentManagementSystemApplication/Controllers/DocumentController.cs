using BusinessObject;
using BusinessObject.Option;
using DataAccess;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly MongoDbService _mongoDbService;
        private readonly ILoggingService _loggingService;
        

        public DocumentController(IDocumentService documentService, IFileService fileService, MongoDbService mongoDbService, ILoggingService loggingService)
        {
            _documentService = documentService;
            _fileService = fileService;
            _mongoDbService = mongoDbService;
            _loggingService = loggingService;
        }
        
        [AllowAnonymous]
        [HttpPost("create-convert-doc-to-pdf")]
        //[AuthorizeResource("[Document] Create Convert Doc To Pdf")]
        public async Task<IActionResult> ConvertDocToPdf(IFormFile file)
        {
            return await _fileService.ConvertDocToPdf(file);
        }
        
        [AllowAnonymous]
        [HttpGet("view-document-elasticsearch")]
        public async Task<ResponseDto> ViewDocumentElasticsearch([FromQuery]string query)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllDocumentElastic(query);
            return result;
        }


        [HttpPost("create-upload-document")]
        //[AuthorizeResource("[Document] Create Upload Document")]
        public async Task<ResponseDto> UploadDocument(IFormFile file)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UploadDocument(file, id);
            return result;
        }

        [HttpPost("create-incoming-document")]
        //[AuthorizeResource("[Document] Create Incoming Document")]
        public async Task<ResponseDto> CreateIncomingDocument([FromBody] DocumentUploadDto documentUploadDto)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.CreateIncomingDoc(documentUploadDto, Guid.Parse(id));
            return result;
        }


        [HttpGet("view-file/{documentId}")]
        //[AuthorizeResource("[Document] View File")]
        public async Task<IActionResult> DownloadDocumentByName([FromRoute] Guid documentId,
            [FromQuery] string? version, [FromQuery] bool isArchive,[FromQuery]bool isDoc=false)
        {
            if (!isArchive)
                return await _documentService.GetDocumentById(documentId, version,isDoc);
          
            return await _documentService.GetArchiveDocumentById(documentId, version);
        }
        
        [HttpGet("create-log-download")]
        public async Task<ResponseDto> CreateLogDownload([FromQuery] Guid documentId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.CreateLogDownload(documentId, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-file-by-name")]
        //[AuthorizeResource("[Document] View File By Name")]
        public async Task<IActionResult> DownloadDocumentByFileName([FromQuery] string documentName)
        {
            var id = User.FindFirst("userid")?.Value;
            return await _documentService.GetDocumentByFileName(documentName, Guid.Parse(id));
        }

        [HttpPost("update-confirm-task-with-document")]
        //[AuthorizeResource("[Document] Update Confirm Task With Document")]
        public async Task<ResponseDto> UpdateConfirmTaskWithDocument([FromQuery] Guid documentId)
        {
            var id = User.FindFirst("userid")?.Value;
            
            var result = await _documentService.UpdateConfirmTaskWithDocument(documentId);
            await _loggingService.WriteLogAsync(Guid.Parse(id), $"Xác nhận task với văn bản có ID:{documentId}");
            return result;
        }

        //[AllowAnonymous]
        [HttpGet("view-all-type-documents-by-workflow-mobile")]
        //[AuthorizeResource("[Document] View All Type Documents By WorkFlow Mobile")]
        public async Task<ResponseDto> ViewAllTypeDocumentsByWorkFlowMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllTypeDocumentsMobile(Guid.Parse(id));
            return result;
        }

        [HttpGet("view-all-type-documents-mobile")]
        //[AuthorizeResource("[Document] View All Type Documents Mobile")]
        public async Task<ResponseDto> ViewAllTypeDocumentsMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllTypeDocMobile(Guid.Parse(id));
            return result;
        }

        [HttpGet("view-all-documents-mobile")]
        //[AuthorizeResource("[Document] View All Documents Mobile")]
        public async Task<ResponseDto> ViewAllDocumentsMobile([FromQuery] Guid? workFlowId,
            [FromQuery] Guid documentTypeId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllDocumentsMobile(workFlowId, documentTypeId, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-detail-documents-mobile")]
        //[AuthorizeResource("[Document] View Detail Documents Mobile")]
        public async Task<ResponseDto> ViewDetailDocumentsMobile([FromQuery] Guid documentId,
            [FromQuery] Guid workFlowId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetDocumentDetailByIdMobile(documentId, Guid.Parse(id), workFlowId);
            return result;
        }

        [HttpGet("view-all-documents-by-document-type-mobile")]
        //[AuthorizeResource("[Document] View All Documents By Document Type Mobile")]
        public async Task<ResponseDto> ViewAllDocumentsByTypeMobile([FromQuery] Guid documentTypeId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetAllDocumentsByTypeMobile(documentTypeId, Guid.Parse(id));
            return result;
        }

        [HttpGet("update-clear-cache-document-mobile")]
        //[AuthorizeResource("[Document] Update Clear Cache Document Mobile")]
        public async Task<ResponseDto> UpdateClearCacheDocumentMobile()
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.ClearCacheDocumentMobile(Guid.Parse(id));
            return result;
        }

        [HttpGet("view-document-by-name")]
        //[AuthorizeResource("[Document] View Document By Name")]
        public async Task<ResponseDto> GetDocumentByNameMobile([FromQuery] string documentName)
        {
            var id = User.FindFirst("userid")?.Value;

            var result = await _documentService.GetDocumentByNameMobile(documentName, Guid.Parse(id));
            return result;
        }

        [HttpGet("view-detail-document")]
        //[AuthorizeResource("[Document] View Detail Document")]
        public async Task<ResponseDto> ViewDetailDocument([FromQuery] Guid documentId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetDocumentDetailById(documentId, Guid.Parse(id));
            return result;
        }

        [HttpPost("view-my-self-document")]
        //[AuthorizeResource("[Document] View My Self Document")]
        public async Task<ResponseDto> ViewMySelfDocument([FromBody] GetAllMySelfRequestDto getAllMySelfRequestDto, [FromQuery] int page = 1,
            int pageSize = 10)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetMySelfDocument(Guid.Parse(id), getAllMySelfRequestDto, page, pageSize);
            return result;
        }

        [HttpGet("view-process-document-detail")]
        //[AuthorizeResource("[Document] View Process Document Detail")]
        public async Task<ResponseDto> ViewProcessDocumentDetail([FromQuery] Guid? documentId)
        {
            var result = await _documentService.ShowProcessDocumentDetail(documentId);
            return result;
        }
        [HttpPost("create-document-by-template")]
        //[AuthorizeResource("[Document] Create Document By Template")]
        public async Task<ResponseDto> CreateDocumentByTemplate([FromBody] DocumentPreInfo documentPreInfo)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.CreateDocumentByTemplate(documentPreInfo, Guid.Parse(id));
            return result;
        }
        
        [HttpPost("create-upload-document-for-submit")]
        //[AuthorizeResource("[Document] Create Upload Document For Submit")]
        public async Task<ResponseDto> UploadDocumentForSubmit([FromForm] DocumentUpload documentUpload)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UploadDocumentForSumit(documentUpload, Guid.Parse(id));
            return result;
        }
        
        [HttpPost("update-confirm-document-by-submit")]
        //[AuthorizeResource("[Document] Update Confirm Document By Submit")]
        public async Task<ResponseDto> UpdateConfirmDocumentBySubmit([FromBody] DocumentCompareDto documentUpload)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UpdateConfirmDocumentBySubmit(documentUpload, Guid.Parse(id));
            return result;
        }   
        
        [HttpGet("view-document-for-usb")]
        //[AuthorizeResource("[Document] View Document For Usb")]
        public async Task<ResponseDto> ViewDocumentForUsb([FromQuery] Guid documentId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.GetDocumentForUsb(documentId, Guid.Parse(id));
            return result;
        }
        
        [HttpPost("update-document-from-usb/{documentId:guid}")]
        //[AuthorizeResource("[Document] Update Document From Usb")]
        public async Task<ResponseDto> UpdateDocumentFromUsb([FromBody] DocumentForSignByUsb documentForSignByUsb,[FromRoute] Guid documentId)
        {
            var id = User.FindFirst("userid")?.Value;
            var result = await _documentService.UpdateDocumentFromUsb(documentForSignByUsb,documentId, Guid.Parse(id));
            return result;
        }

        [HttpPost("create-upload-attachment")]
        public async Task<ResponseDto> UploadAttachment(IFormFile attachmentDocumentRequest)
        {
            var result = await _documentService.UploadAttachment(attachmentDocumentRequest);
            return result;
        }

        [HttpGet("view-attach-file/{documentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewAttachFile([FromRoute] Guid documentId)
        {
            return await _fileService.GetAttachFileById(documentId);
        }

    }
}
