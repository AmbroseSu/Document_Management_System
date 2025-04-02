using DataAccess.DTO;

namespace Service;

public interface IDocumentTypeService
{
    Task<ResponseDto> AddDocumentTypeAsync(string? documentTypeName);
    Task<ResponseDto> UpdateDocumentTypeAsync(DocumentTypeDto documentTypeDto);
    Task<ResponseDto> GetAllDocumentTypeAsync(int page, int limit);
    Task<ResponseDto> UpdateDocumentTypeActiveOrDeleteAsync(Guid documentTypeId);
    Task<ResponseDto> GetDocumentTypeDetails(Guid documentTypeId);
}