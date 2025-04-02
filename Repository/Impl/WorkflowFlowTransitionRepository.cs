using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class WorkflowFlowTransitionRepository : IWorkflowFlowTransitionRepository
{
    private readonly BaseDao<WorkflowFlowTransition> _workflowFlowTransitionDao;

    public WorkflowFlowTransitionRepository(DocumentManagementSystemDbContext context)
    {
        _workflowFlowTransitionDao = new BaseDao<WorkflowFlowTransition>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(WorkflowFlowTransition entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _workflowFlowTransitionDao.AddAsync(entity);
    }
    
    public async Task AddRangeAsync(List<WorkflowFlowTransition> workflowFlowTransitions)
    {
        if (workflowFlowTransitions == null) throw new ArgumentNullException(nameof(workflowFlowTransitions));
        await _workflowFlowTransitionDao.AddRangeAsync(workflowFlowTransitions);
    }

    public async Task UpdateAsync(WorkflowFlowTransition entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _workflowFlowTransitionDao.UpdateAsync(entity);
    }
    
    public async Task UpdateRangeAsync(List<WorkflowFlowTransition> workflowFlowTransitions)
    {
        if (workflowFlowTransitions == null) throw new ArgumentNullException(nameof(workflowFlowTransitions));
        await _workflowFlowTransitionDao.UpdateRangeAsync(workflowFlowTransitions);
    }
    
    public async Task<IEnumerable<WorkflowFlowTransition>> FindByWorkflowFlowIdsAsync(List<Guid> workflowFlowIds)
    {
        return await _workflowFlowTransitionDao.FindAsync(wft => workflowFlowIds.Contains(wft.CurrentWorkFlowFlowId) 
                                                                || workflowFlowIds.Contains(wft.NextWorkFlowFlowId));
    }
    
}