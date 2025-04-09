using DataAccess.DTO;
using Microsoft.AspNetCore.Http;

namespace Service;

public interface IDocumentService
{ 
    Task<ResponseDto> UploadDocument(IFormFile file,Guid userId);
    Task<ResponseDto> InsertSimpleDocument(DocumentDto document);
}