using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Workflow
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    
    public Guid DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    
    public List<Step>? Steps { get; set; }

    /*public Workflow()
    {
        WorkflowId = Guid.NewGuid();
    }*/
}