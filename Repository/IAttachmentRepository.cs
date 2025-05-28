using BusinessObject;

namespace Repository;

public interface IAttachmentRepository
{
    
    Task AddAsync(AttachmentDocument entity);
    Task UpdateAsync(AttachmentDocument entity);
    Task<AttachmentDocument?> FindAttachmentDocumentByIdAsync(Guid? id);
    Task<AttachmentDocument?> FindAttachmentDocumentByNameAsync(string? name);
    Task<IEnumerable<AttachmentDocument>> FindAllAttachmentDocumentAsync();
    Task<IEnumerable<AttachmentDocument>> FindAttachmentDocumentsByIdsAsync(List<Guid> AttachmentDocumentIds);
    Task<IEnumerable<AttachmentDocument>> GetAttachmentDocumentByDocumentId(Guid documentId);
}