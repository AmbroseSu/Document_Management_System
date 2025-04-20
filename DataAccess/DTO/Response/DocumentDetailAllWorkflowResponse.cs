using DataAccess.DTO.Request;

namespace DataAccess.DTO.Response;

public class DocumentDetailAllWorkflowResponse
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentTypeName { get; set; }
    public WorkflowRequest WorkflowRequest { get; set; }
}