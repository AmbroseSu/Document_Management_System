using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IDocumentService
{ 
    Task<ResponseDto> UploadDocument(IFormFile file, string? userId);
    Task<ResponseDto> CreateDoc(DocumentUploadDto documentUploadDto, Guid userId);
    Task<IActionResult> GetDocumentById(Guid documentId);
    Task<IActionResult> GetDocumentByName(string documentName);
}