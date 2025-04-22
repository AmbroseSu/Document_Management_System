using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Http;

namespace Service;

public interface IArchiveDocumentService
{
    //string ExtractSignatures(IFormFile file);
    Task<ResponseDto> GetAllArchiveDocuments(Guid userId,int page,int pageSize);
    Task<ResponseDto> GetAllArchiveTemplates(int page, int pageSize);
    Task<ResponseDto> GetArchiveDocumentDetail(Guid documentId, Guid userId);
    Task<ResponseDto> CreateArchiveTemplate(ArchiveDocumentRequest archiveDocumentRequest, Guid userId);
}