using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentFileExtension
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentFileExtensionId { get; set; }
    public string? DocumentFileExtensionName { get; set; }
    public bool IsDeleted { get; set; }
    
    public List<Document>? Documents { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }

    /*public DocumentFileExtension()
    {
        DocumentFileExtensionId = Guid.NewGuid();
    }*/
}