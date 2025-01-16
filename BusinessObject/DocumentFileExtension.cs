using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentFileExtension
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int DocumentFileExtensionId { get; set; }
    public string? DocumentFileExtensionName { get; set; }
    public bool IsDeleted { get; set; }
    
    public List<Document>? Documents { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }
}