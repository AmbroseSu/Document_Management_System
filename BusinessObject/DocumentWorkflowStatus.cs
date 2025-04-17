using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BusinessObject.Enums;

namespace BusinessObject;

public class DocumentWorkflowStatus
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentWorkflowStatusId { get; set; }
    public StatusDocWorkflow StatusDocWorkflow { get; set; }
    public StatusDoc StatusDoc { get; set; }
    private DateTime _updatedAt;
    public DateTime UpdatedAt
    {
        get => DateTime.SpecifyKind(_updatedAt, DateTimeKind.Utc).ToLocalTime();
        set => _updatedAt = value.ToUniversalTime();
    }
    
    public Guid DocumentId { get; set; }
    [JsonIgnore]
    public Document? Document { get; set; }
    public Guid WorkflowId { get; set; }
    [JsonIgnore]
    public Workflow? Workflow { get; set; }
    public Guid CurrentWorkflowFlowId { get; set; }
    [JsonIgnore]
    public WorkflowFlow? CurrentWorkflowFlow { get; set; }
}