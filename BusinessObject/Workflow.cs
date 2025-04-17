using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class Workflow
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid WorkflowId { get; set; }

    public string? WorkflowName { get; set; }
    public Scope Scope { get; set; }
    public string RequiredRolesJson { get; set; }
    private DateTime? _createAt;
    public DateTime? CreateAt
    {
        get => _createAt.HasValue ? DateTime.SpecifyKind(_createAt.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _createAt = value?.ToUniversalTime();
    }
    public Guid? CreateBy { get; set; }
    public bool IsAllocate { get; set; }
    public bool IsDeleted { get; set; } 

    public List<WorkflowFlow>? WorkflowFlows { get; set; }
    public List<DocumentTypeWorkflow>? DocumentTypeWorkflows { get; set; }
    public List<DocumentWorkflowStatus>? DocumentWorkflowStatuses { get; set; }

    /*public Workflow()
    {
        WorkflowId = Guid.NewGuid();
    }*/
}