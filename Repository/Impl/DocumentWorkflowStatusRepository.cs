using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository.Impl;

public class DocumentWorkflowStatusRepository : IDocumentWorkflowStatusRepository
{
    private readonly BaseDao<DocumentWorkflowStatus> _documentWorkflowStatusDao;

    public DocumentWorkflowStatusRepository(DocumentManagementSystemDbContext context)
    {
        _documentWorkflowStatusDao = new BaseDao<DocumentWorkflowStatus>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(DocumentWorkflowStatus entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentWorkflowStatusDao.AddAsync(entity);
    }

    public async Task UpdateAsync(DocumentWorkflowStatus entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentWorkflowStatusDao.UpdateAsync(entity);
    }

    public async Task<DocumentWorkflowStatus?> FindDocumentWorkflowStatusByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _documentWorkflowStatusDao.FindByAsync(u => u.DocumentWorkflowStatusId == id);
    }
    
    public async Task<IEnumerable<DocumentWorkflowStatus>> FindAllDocumentWorkflowStatusAsync()
    {
        return await _documentWorkflowStatusDao.FindAsync(u => true);
    }
}