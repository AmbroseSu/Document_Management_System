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
    public string? DocumentContent { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SignedBy { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public string? Sender { get; set; }
    public string? DateReceived { get; set; }
    public string? DateIssued { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid DocumentTypeId { get; set; }
    public Guid DeadlineId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public Deadline? Deadline { get; set; }
    public List<Task>? Tasks { get; set; }
    public List<UserDocument>? UserDocuments { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }

    /*public Document()
    {
        DocumentId = Guid.NewGuid();
    }*/
}