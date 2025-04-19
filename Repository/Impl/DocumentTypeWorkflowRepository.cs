using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository.Impl;

public class DocumentTypeWorkflowRepository : IDocumentTypeWorkflowRepository
{
    private readonly BaseDao<DocumentTypeWorkflow> _documentTypeWorkflowDao;

    public DocumentTypeWorkflowRepository(DocumentManagementSystemDbContext context)
    {
        _documentTypeWorkflowDao = new BaseDao<DocumentTypeWorkflow>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    
    public async Task AddAsync(DocumentTypeWorkflow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentTypeWorkflowDao.AddAsync(entity);
    }
    
    public async Task UpdateAsync(DocumentTypeWorkflow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentTypeWorkflowDao.UpdateAsync(entity);
    }
    
    public async Task AddRangeAsync(List<DocumentTypeWorkflow> documentTypeWorkflow)
    {
        if (documentTypeWorkflow == null) throw new ArgumentNullException(nameof(documentTypeWorkflow));
        await _documentTypeWorkflowDao.AddRangeAsync(documentTypeWorkflow);
    }
    
    public async Task<IEnumerable<DocumentTypeWorkflow>> FindAllDocumentTypeNameByWorkflowIdAsync(Guid? workflowId)
    {
        if (workflowId == null) throw new ArgumentNullException(nameof(workflowId));
        return await _documentTypeWorkflowDao.FindAsync(u => u.WorkflowId == workflowId && u.DocumentType.IsDeleted == false,
            u => u.Include(d => d.Workflow)
                .Include(d => d.DocumentType));
    }
    
}