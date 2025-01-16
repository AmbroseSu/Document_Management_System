using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class AttachmentDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int AttachmentDocumentId { get; set; }
    public string? AttachmentDocumentName { get; set; }
    public string? AttachmentDocumentUrl { get; set; }
    public bool IsDeleted { get; set; }
    
    public int DocumentId { get; set; }
    public int DocumentFileExtensionId { get; set; }
    public Document? Document { get; set; }
    public DocumentFileExtension? DocumentFileExtension { get; set; }
    
}