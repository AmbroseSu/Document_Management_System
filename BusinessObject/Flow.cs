using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Flow
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid FlowId { get; set; }

    public int FlowNumber { get; set; }

    public Guid WorkflowId { get; set; }
    public Workflow? Workflow { get; set; }

    public List<Step>? Steps { get; set; }
}