using BusinessObject;

namespace Repository;

public interface IDocumentTypeWorkflowRepository
{
    Task AddAsync(DocumentTypeWorkflow entity);
    Task UpdateAsync(DocumentTypeWorkflow entity);
    Task AddRangeAsync(List<DocumentTypeWorkflow> documentTypeWorkflow);
    Task<IEnumerable<DocumentTypeWorkflow>> FindAllDocumentTypeNameByWorkflowIdAsync(Guid? workflowId);

}