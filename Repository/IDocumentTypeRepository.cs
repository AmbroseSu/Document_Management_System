using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IDocumentTypeRepository
{
    Task AddAsync(DocumentType entity);
    Task UpdateAsync(DocumentType entity);
    Task<DocumentType?> FindDocumentTypeByIdAsync(Guid? id);
    Task<DocumentType?> FindDocumentTypeByNameAsync(string? name);
    Task<IEnumerable<DocumentType>> FindAllDocumentTypeAsync();
}