using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IWorkflowFlowRepository
{
    Task AddAsync(WorkflowFlow entity);
    Task AddRangeAsync(List<WorkflowFlow> workflowFlows);
    Task UpdateAsync(WorkflowFlow entity);
    Task<IEnumerable<WorkflowFlow>> FindWorkflowFlowByWorkflowIdAsync(Guid? id);
    Task<WorkflowFlow?> FindWorkflowFlowByFlowIdAsync(Guid? id);
    Task<WorkflowFlow> FindWorkflowFlowByWorkflowIdAndFlowIdAsync(Guid? workflowId, Guid? flowId);
}