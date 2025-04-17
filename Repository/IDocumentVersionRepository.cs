using BusinessObject;

namespace Repository;

public interface IDocumentVersionRepository
{
    Task AddAsync(DocumentVersion documentVersion);
    Task UpdateAsync(DocumentVersion documentVersion);
    Task<DocumentVersion?> FindDocumentVersionByIdAsync(Guid? documentVersionId);
    Task<IEnumerable<DocumentVersion>?> FindDocumentVersionByDocumentIdAsync(Guid? documentId);
}