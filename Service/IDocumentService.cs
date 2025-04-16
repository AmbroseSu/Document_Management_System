using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IDocumentService
{ 
    Task<ResponseDto> UploadDocument(IFormFile file, string? userId);
    Task<ResponseDto> CreateIncomingDoc(DocumentUploadDto documentUploadDto, Guid userId);
    Task<IActionResult> GetDocumentById(Guid documentId,string version);
    // Task<IActionResult> GetDocumentByName(string documentName);
    Task<ResponseDto> UpdateConfirmTaskWithDocument(Guid documentId);
    Task<IActionResult> GetArchiveDocumentById(Guid documentId, string? version);
    Task<ResponseDto> GetAllTypeDocumentsMobile(Guid userId);
    Task<ResponseDto> GetAllDocumentsMobile(Guid? workFlowId, Guid documentTypeId, Guid userId);
    Task<ResponseDto> GetDocumentDetailById(Guid? documentId, Guid userId,Guid workFlowId);
    Task<ResponseDto> ClearCacheDocumentMobile(Guid userId);
    Task<ResponseDto> GetAllTypeDocMobile(Guid userId);
    Task<ResponseDto> GetAllDocumentsByTypeMobile(Guid documentTypeId, Guid userId);
    Task<ResponseDto> GetDocumentByNameMobile(string documentName, Guid userId);
}