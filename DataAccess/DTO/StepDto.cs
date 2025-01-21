namespace DataAccess.DTO;

public class StepDto
{
    public Guid StepId { get; set; }
    public int StepNumber { get; set; }
    public string? Action { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid? WorkflowId { get; set; }
    public Guid? RoleId { get; set; }
    public List<Guid>? TaskIds { get; set; }
}