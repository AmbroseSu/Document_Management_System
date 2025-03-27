using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentType
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentTypeId { get; set; }

    public string? DocumentTypeName { get; set; }
    public bool IsDeleted { get; set; }

    public List<DocumentTypeWorkflow>? DocumentTypeWorkflows { get; set; }
    public List<ArchivedDocument>? ArchivedDocuments { get; set; }

    /*public DocumentType()
    {
        DocumentTypeId = Guid.NewGuid();
    }*/
}