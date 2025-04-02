using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IWorkflowFlowTransitionRepository
{
    Task AddAsync(WorkflowFlowTransition entity);
    Task AddRangeAsync(List<WorkflowFlowTransition> workflowFlowTransitions);
    Task UpdateAsync(WorkflowFlowTransition entity);
    Task UpdateRangeAsync(List<WorkflowFlowTransition> workflowFlowTransitions);
    Task<IEnumerable<WorkflowFlowTransition>> FindByWorkflowFlowIdsAsync(List<Guid> workflowFlowIds);
}