using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class WorkflowFlowRepository : IWorkflowFlowRepository
{
    private readonly BaseDao<WorkflowFlow> _workflowFlowDao;

    public WorkflowFlowRepository(DocumentManagementSystemDbContext context)
    {
        _workflowFlowDao = new BaseDao<WorkflowFlow>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(WorkflowFlow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _workflowFlowDao.AddAsync(entity);
    }
    
    public async Task AddRangeAsync(List<WorkflowFlow> workflowFlows)
    {
        if (workflowFlows == null) throw new ArgumentNullException(nameof(workflowFlows));
        await _workflowFlowDao.AddRangeAsync(workflowFlows);
    }

    public async Task UpdateAsync(WorkflowFlow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _workflowFlowDao.UpdateAsync(entity);
    }
    public async Task<IEnumerable<WorkflowFlow>?> FindWorkflowFlowByWorkflowIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _workflowFlowDao.FindAsync(wf => wf.WorkflowId == id, wff => wff.Include(wf => wf.Flow).ThenInclude(f => f.Steps));
    }
    
    public async Task<WorkflowFlow?> FindWorkflowFlowByFlowIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _workflowFlowDao.FindByAsync(wf => wf.FlowId == id);
    }
    
    public async Task<WorkflowFlow> FindWorkflowFlowByWorkflowIdAndFlowIdAsync(Guid? workflowId, Guid? flowId)
    {
        if (workflowId == null) throw new ArgumentNullException(nameof(workflowId));
        if (flowId == null) throw new ArgumentNullException(nameof(flowId));
        return await _workflowFlowDao.FindByAsync(wf => wf.WorkflowId == workflowId && wf.FlowId == flowId, wff => wff.Include(w => w.Workflow).Include(wf => wf.Flow).ThenInclude(f => f.Steps));
    }
}