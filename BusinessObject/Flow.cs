using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class Flow
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid FlowId { get; set; }

    public string? RoleStart { get; set; }
    public string? RoleEnd { get; set; }
    public Scope ScopeFlow { get; set; }
    
    public List<WorkflowFlow>? WorkflowFlows { get; set; }
    public List<Step>? Steps { get; set; }

}