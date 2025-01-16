using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class AttachmentArchivedDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int AttachmentArchivedDocumentId { get; set; }
    public string? AttachmentName { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsDeleted { get; set; }
    
    public int ArchivedDocumentId { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }
}