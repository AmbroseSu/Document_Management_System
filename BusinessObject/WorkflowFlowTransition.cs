using BusinessObject.Enums;

namespace BusinessObject;

public class WorkflowFlowTransition
{
    public Guid WorkflowFlowTransitionId { get; set; }
    public Guid CurrentWorkFlowFlowId { get; set; }
    public Guid NextWorkFlowFlowId { get; set; }
    public WorkflowFlow? CurrentWorkFlowFlow { get; set; }
    public WorkflowFlow? NextWorkFlowFlow { get; set; }
    public FlowTransitionCondition Condition { get; set; }
    public bool IsDeleted { get; set; }
}