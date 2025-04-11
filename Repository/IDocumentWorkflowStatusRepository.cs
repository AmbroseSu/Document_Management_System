using BusinessObject;

namespace Repository;

public interface IDocumentWorkflowStatusRepository
{
    Task AddAsync(DocumentWorkflowStatus entity);
    Task UpdateAsync(DocumentWorkflowStatus entity);
    Task<DocumentWorkflowStatus?> FindDocumentWorkflowStatusByIdAsync(Guid? id);
    // Task<DocumentWorkflowStatus?> FindDocumentWorkflowStatusByNameAsync(string? name);
    Task<IEnumerable<DocumentWorkflowStatus>> FindAllDocumentWorkflowStatusAsync();
}