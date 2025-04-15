using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class DocumentTypeWorkflow
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentTypeWorkflowId { get; set; }

    public Guid DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }

    public Guid WorkflowId { get; set; }
    [JsonIgnore]
    public Workflow? Workflow { get; set; }
    
}