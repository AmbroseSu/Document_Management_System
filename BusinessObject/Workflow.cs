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
    public bool IsAllocate { get; set; }

    public List<Flow>? Flows { get; set; }
    public List<DocumentTypeWorkflow>? DocumentTypeWorkflows { get; set; }

    /*public Workflow()
    {
        WorkflowId = Guid.NewGuid();
    }*/
}