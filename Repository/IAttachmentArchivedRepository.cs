using BusinessObject;

namespace Repository;

public interface IAttachmentArchivedRepository
{
    Task AddAsync(AttachmentArchivedDocument entity);
    Task UpdateAsync(AttachmentArchivedDocument entity);
    Task<AttachmentArchivedDocument?> FindAttachmentArchivedDocumentByIdAsync(Guid? id);
    Task<AttachmentArchivedDocument?> FindAttachmentArchivedDocumentByNameAsync(string? name);
    Task<IEnumerable<AttachmentArchivedDocument>> FindAllAttachmentArchivedDocumentAsync();
    Task<IEnumerable<AttachmentArchivedDocument>> FindAttachmentArchivedDocumentsByIdsAsync(List<Guid> AttachmentArchivedDocumentIds);
    Task<IEnumerable<AttachmentArchivedDocument>> GetAttachmentArchivedDocumentByDocumentId(Guid documentId);
}