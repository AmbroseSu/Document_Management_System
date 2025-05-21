namespace DataAccess.DTO.Response;

public class WorkflowScopeResponse
{
    public Guid WorkflowId { get; set; }
    public String WorkflowName { get; set; }
    public String FirstRole { get; set; }
    public List<DocumentTypeDto> DocumentTypes { get; set; }
}