using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IDocumentService
{ 
    Task<ResponseDto> UploadDocument(IFormFile file, string? userId);
    Task<ResponseDto> CreateIncomingDoc(DocumentUploadDto documentUploadDto, Guid userId);
    Task<IActionResult> GetDocumentById(Guid documentId,string version,bool isDoc);
    // Task<IActionResult> GetDocumentByName(string documentName);
    Task<ResponseDto> UpdateConfirmTaskWithDocument(Guid documentId);
    Task<IActionResult> GetArchiveDocumentById(Guid documentId, string? version);
    Task<ResponseDto> GetAllTypeDocumentsMobile(Guid userId);
    Task<ResponseDto> GetAllDocumentsMobile(Guid? workFlowId, Guid documentTypeId, Guid userId);
    Task<ResponseDto> GetDocumentDetailByIdMobile(Guid? documentId, Guid userId,Guid workFlowId);
    Task<ResponseDto> ClearCacheDocumentMobile(Guid userId);
    Task<ResponseDto> GetAllTypeDocMobile(Guid userId);
    Task<ResponseDto> GetAllDocumentsByTypeMobile(Guid documentTypeId, Guid userId);
    Task<ResponseDto> GetDocumentByNameMobile(string documentName, Guid userId);
    Task<ResponseDto> GetDocumentDetailById(Guid documentId, Guid userId);
    Task<ResponseDto> GetMySelfDocument(Guid userId, GetAllMySelfRequestDto getAllMySelfRequestDto, int page, int pageSize);
    Task<IActionResult> GetDocumentByFileName(string documentName, Guid userId);
    Task<ResponseDto> ShowProcessDocumentDetail(Guid? documentId);
    void AddFooterToPdf(string inputFilePath, string outputFilePath);
    List<MetaDataDocument>? CheckMetaDataFile(string url);

    Task<ResponseDto> CreateDocumentByTemplate(DocumentPreInfo documentPreInfo, Guid userId);
    Task<ResponseDto> UploadDocumentForSumit(DocumentUpload documentUpload, Guid userId);
    Task<ResponseDto> UpdateConfirmDocumentBySubmit(DocumentCompareDto documentUpload, Guid userId);
    Task<ResponseDto> GetDocumentForUsb(Guid documentId, Guid userId);
    Task<ResponseDto> UpdateDocumentFromUsb(DocumentForSignByUsb documentForSignByUsb,Guid documentId, Guid userId);
    Task<ResponseDto> GetAllDocumentElastic(string query);
}