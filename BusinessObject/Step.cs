using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class Step
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid StepId { get; set; }

    public int StepNumber { get; set; }
    public string? Action { get; set; }
    public Guid? NextStepId { get; set; }
    public Guid? RejectStepId { get; set; }
    public bool IsDeleted { get; set; }

    public Guid FlowId { get; set; }
    public Guid RoleId { get; set; }
    [JsonIgnore]
    public Flow? Flow { get; set; }
    public Role? Role { get; set; }

    public List<Tasks>? Tasks { get; set; }

    /*public Step()
    {
        StepId = Guid.NewGuid();
    }*/
}