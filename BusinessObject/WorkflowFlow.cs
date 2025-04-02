using BusinessObject.Enums;

namespace BusinessObject;

public class WorkflowFlow
{
    public Guid WorkflowFlowId { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid FlowId { get; set; }
    public Workflow? Workflow { get; set; }
    public Flow? Flow { get; set; }
    public int FlowNumber { get; set; }
    public WorkflowFlowStatus Status { get; set; }
    
    public List<WorkflowFlowTransition>? CurrentWorkflowFlowTransitions { get; set; }
    public List<WorkflowFlowTransition>? NextWorkflowFlowTransitions { get; set; }
    public List<DocumentWorkflowStatus>? DocumentWorkflowStatuses { get; set; }
}