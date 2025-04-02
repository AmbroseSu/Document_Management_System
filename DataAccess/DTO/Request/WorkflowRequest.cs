using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class WorkflowRequest
{
    public Guid? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public Scope Scope { get; set; }
    public bool IsAllocate { get; set; }
    public Guid CreateBy { get; set; }
    public List<FlowDto>? Flows { get; set; }
    
}