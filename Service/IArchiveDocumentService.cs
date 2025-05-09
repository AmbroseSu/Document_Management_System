using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IArchiveDocumentService
{
    //string ExtractSignatures(IFormFile file);
    Task<ResponseDto> GetAllArchiveDocuments(GetAllArchiveRequestDto getAllArchiveRequestDto, Guid userId,int page,int pageSize);
    Task<ResponseDto> GetAllArchiveTemplates(string? documentType,string? name,int page, int pageSize);
    Task<ResponseDto> GetArchiveDocumentDetail(Guid documentId, Guid userId);
    Task<ResponseDto> CreateArchiveTemplate(ArchiveDocumentRequest archiveDocumentRequest, Guid userId);
    Task<IActionResult> DownloadTemplate(string templateId, Guid userId,bool? isPdf);
    Task<ResponseDto> WithdrawArchiveDocument(Guid archiveDocumentId,DocumentPreInfo documentPreInfo, Guid userId);
    Task<ResponseDto> ReplaceArchiveDocument(Guid archiveDocumentId, DocumentPreInfo documentPreInfo, Guid userId);
    Task<ResponseDto> DeleteArchiveTemplate(Guid templateId, Guid userId);
}