namespace DataAccess.DTO;

public class WorkflowDto
{
    public Guid WorkflowId { get; set; }
    public string? WorkflowName { get; set; }

    public Guid? DocumentTypeId { get; set; }
    public List<Guid>? StepIds { get; set; }
}