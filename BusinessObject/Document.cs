using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class Document
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DocumentStatus DocumentStatus { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    
    public int DocumentTypeId { get; set; }
    public int DeadlineId { get; set; }
    public int DocumentFileExtensionId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public Deadline? Deadline { get; set; }
    public DocumentFileExtension? DocumentFileExtension { get; set; }
    
    public List<Task>? Tasks { get; set; }
    public List<UserDocument>? UserDocuments { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }
    
    
}