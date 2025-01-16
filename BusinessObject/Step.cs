using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Step
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int StepId { get; set; }
    public int StepNumber { get; set; }
    public string? Action { get; set; }
    public bool IsDeleted { get; set; }
    
    public int WorkflowId { get; set; }
    public int RoleId { get; set; }
    public Workflow? Workflow { get; set; }
    public Role? Role { get; set; }
    
    public List<Task>? Tasks { get; set; }
    
    
}