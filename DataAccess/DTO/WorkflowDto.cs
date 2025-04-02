using BusinessObject.Enums;

namespace DataAccess.DTO;

public class WorkflowDto
{
    public Guid WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public Scope Scope { get; set; }
    public bool IsAllocate { get; set; }
    public bool IsDeleted { get; set; }

    //public Guid? DocumentTypeId { get; set; }
    //public List<Guid>? StepIds { get; set; }
}