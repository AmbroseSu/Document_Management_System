using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class ArchivedDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int ArchivedDocumentId { get; set; }
    public string? ArchivedDocumentName { get; set; }
    public string? ArchivedDocumentUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public int DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    
    public List<UserDocumentPermission>? UserDocumentPermissions { get; set; }
    public List<AttachmentArchivedDocument>? AttachmentArchivedDocuments { get; set; }
}