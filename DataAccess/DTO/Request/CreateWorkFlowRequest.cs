using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class CreateWorkFlowRequest
{
    public Guid? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public Scope Scope { get; set; }
    public Guid CreateBy { get; set; }
    public List<Guid> DocumentTypeIds { get; set; }
    public List<Guid> FlowIds { get; set; }
    
}