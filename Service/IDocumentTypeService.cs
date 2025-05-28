using DataAccess.DTO;

namespace Service;

public interface IDocumentTypeService
{
    Task<ResponseDto> AddDocumentTypeAsync(DocumentTypeDto documentTypeDto, Guid userId);
    Task<ResponseDto> UpdateDocumentTypeAsync(DocumentTypeDto documentTypeDto, Guid userId);
    Task<ResponseDto> GetAllDocumentTypeAsync(string? documentTypeName, int page, int limit);
    Task<ResponseDto> UpdateDocumentTypeActiveOrDeleteAsync(Guid documentTypeId, Guid userId);
    Task<ResponseDto> GetDocumentTypeDetails(Guid documentTypeId);
    Task<ResponseDto> GetAllDocumentTypeNameByWorkflowIdAsync(Guid workflowId);
}