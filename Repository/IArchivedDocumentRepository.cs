using BusinessObject;

namespace Repository;

public interface IArchivedDocumentRepository
{
    Task AddAsync(ArchivedDocument entity);
    Task UpdateAsync(ArchivedDocument entity);
    Task<ArchivedDocument?> FindArchivedDocumentByIdAsync(Guid? id);
    Task<ArchivedDocument?> FindArchivedDocumentByNameAsync(string? name);
    Task<IEnumerable<ArchivedDocument>> FindAllArchivedDocumentAsync();
    Task<IEnumerable<ArchivedDocument>> FindArchivedDocumentsByIdsAsync(List<Guid> archivedDocumentIds);
    Task<IEnumerable<ArchivedDocument>> FindArchivedDocumentByUserIdAsync(Guid userId);
}