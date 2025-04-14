using BusinessObject;
using BusinessObject.Enums;

namespace DataAccess.DTO;

public class DocumentDto
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentContent { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SignedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? Deadline { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public string? Sender { get; set; }
    public DateTime? DateReceived { get; set; }
    public string? DateIssued { get; set; }
    public bool? IsDeleted { get; set; }
    
    public Guid? UserId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? TemplateArchiveDocumentId { get; set; }
    public Guid? FinalArchiveDocumentId { get; set; }
    public List<Tasks>? Tasks { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }
    public List<DocumentVersion>? DocumentVersions { get; set; }
    public List<DocumentWorkflowStatus>? DocumentWorkflowStatuses { get; set; }
}