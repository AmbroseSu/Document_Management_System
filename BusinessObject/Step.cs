using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Step
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid StepId { get; set; }
    public int StepNumber { get; set; }
    public string? Action { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid WorkflowId { get; set; }
    public Guid RoleId { get; set; }
    public Workflow? Workflow { get; set; }
    public Role? Role { get; set; }
    
    public List<Task>? Tasks { get; set; }

    /*public Step()
    {
        StepId = Guid.NewGuid();
    }*/
}