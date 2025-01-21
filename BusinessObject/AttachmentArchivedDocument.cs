using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class AttachmentArchivedDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid AttachmentArchivedDocumentId { get; set; }
    public string? AttachmentName { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid ArchivedDocumentId { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }
    
    /*public AttachmentArchivedDocument()
    {
        AttachmentArchivedDocumentId = Guid.NewGuid();
    }*/
    
}