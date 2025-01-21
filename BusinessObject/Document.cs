using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class Document
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public DocumentStatus DocumentStatus { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public bool IsTemplate { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    
    public Guid DocumentTypeId { get; set; }
    public Guid DeadlineId { get; set; }
    public Guid DocumentFileExtensionId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public Deadline? Deadline { get; set; }
    public DocumentFileExtension? DocumentFileExtension { get; set; }
    
    public List<Task>? Tasks { get; set; }
    public List<UserDocument>? UserDocuments { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }

    /*public Document()
    {
        DocumentId = Guid.NewGuid();
    }*/
}