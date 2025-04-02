using BusinessObject;
using BusinessObject.Enums;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly BaseDao<Workflow> _workflowDao;

    public WorkflowRepository(DocumentManagementSystemDbContext context)
    {
        _workflowDao = new BaseDao<Workflow>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(Workflow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _workflowDao.AddAsync(entity);
    }

    public async Task UpdateAsync(Workflow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _workflowDao.UpdateAsync(entity);
    }

    public async Task<Workflow?> FindWorkflowByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _workflowDao.FindByAsync(u => u.WorkflowId == id);
    }
    
    public async Task<Workflow?> FindWorkflowByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _workflowDao.FindByAsync(u => u.WorkflowName!.ToLower().Equals(name.ToLower()));
    }
    
    public async Task<Workflow?> FindWorkflowByScopeAsync(Scope? scope)
    {
        if (scope == null) throw new ArgumentNullException(nameof(scope));
        return await _workflowDao.FindByAsync(u => u.Scope.Equals(scope));
    }

    public async Task<IEnumerable<Workflow>> FindAllWorkflowAsync()
    {
        return await _workflowDao.FindAsync(u => true,
            u => u.Include(d => d.DocumentTypeWorkflows).Include(d => d.WorkflowFlows));
    }
}