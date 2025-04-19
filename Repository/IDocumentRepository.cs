
using BusinessObject;

namespace Repository;

public interface IDocumentRepository
{
    Task AddAsync(Document entity);
    Task UpdateAsync(Document entity);
    Task<Document?> FindDocumentByIdAsync(Guid? id);
    Task<Document?> FindDocumentByNameAsync(string? name);
    Task<IEnumerable<Document>> FindAllDocumentAsync();
    Task<IEnumerable<Document>> FindAllDocumentForTaskAsync(Guid userId);
    Task<IEnumerable<Document>> FindDocumentByUserIdAsync(Guid userId);
    Task<IEnumerable<Document>> FindAllDocumentMySelf(Guid userId);
}