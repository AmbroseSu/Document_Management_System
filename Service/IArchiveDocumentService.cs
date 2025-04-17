using DataAccess.DTO;
using Microsoft.AspNetCore.Http;

namespace Service;

public interface IArchiveDocumentService
{
    //string ExtractSignatures(IFormFile file);
    Task<ResponseDto> GetAllArchiveDocuments(Guid userId,int page,int pageSize);
}