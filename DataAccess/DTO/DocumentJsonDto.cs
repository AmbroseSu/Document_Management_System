using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject;
using BusinessObject.Enums;

namespace DataAccess.DTO;

public class DocumentJsonDto
{
    private DateTime _createdDate;
    private DateTime _updatedDate;
    private DateTime _deadline;
    private DateTime? _dateReceived;
    private DateTime? _dateIssued;


    public Guid DocumentId { get; set; }

    public string? DocumentName { get; set; }
    public string? DocumentContent { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SignedBy { get; set; }

    public DateTime CreatedDate
    {
        get => DateTime.SpecifyKind(_createdDate, DateTimeKind.Utc).ToLocalTime();
        set => _createdDate = value.ToUniversalTime();
    }
    public DateTime UpdatedDate
    {
        get => DateTime.SpecifyKind(_updatedDate, DateTimeKind.Utc).ToLocalTime();
        set => _updatedDate = value.ToUniversalTime();
    }

    public DateTime Deadline
    {
        get => DateTime.SpecifyKind(_deadline, DateTimeKind.Utc).ToLocalTime();
        set => _deadline = value.ToUniversalTime();
    }

    public ProcessingStatus ProcessingStatus { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public string? Sender { get; set; }

    public DateTime? DateReceived
    {
        get => _dateReceived.HasValue ? DateTime.SpecifyKind(_dateReceived.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _dateReceived = value.HasValue ? value.Value.ToUniversalTime() : default;
    }
    public DateTime? DateIssued 
    {
        get => _dateIssued.HasValue ? DateTime.SpecifyKind(_dateIssued.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _dateIssued = value.HasValue ? value.Value.ToUniversalTime() : default;
    }
    public bool IsDeleted { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? TemplateArchiveDocumentId { get; set; }
    
    public ArchivedDocument? TemplateArchiveDocument { get; set; }
    public Guid? FinalArchiveDocumentId { get; set; }
    public ArchivedDocument? FinalArchiveDocument { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }

    public List<Tasks>? Tasks { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }
    public List<DocumentVersion>? DocumentVersions { get; set; }
    public List<DocumentWorkflowStatusJsonDto>? DocumentWorkflowStatuses { get; set; }
}

public class DocumentWorkflowStatusJsonDto
{

    public Guid DocumentWorkflowStatusId { get; set; }
    public StatusDocWorkflow StatusDocWorkflow { get; set; }
    public StatusDoc StatusDoc { get; set; }
    private DateTime _updatedAt;
    public DateTime UpdatedAt
    {
        get => DateTime.SpecifyKind(_updatedAt, DateTimeKind.Utc).ToLocalTime();
        set => _updatedAt = value.ToUniversalTime();
    }
    
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public Guid WorkflowId { get; set; }
    public Workflow? Workflow { get; set; }
    public Guid CurrentWorkflowFlowId { get; set; }
    public WorkflowFlow? CurrentWorkflowFlow { get; set; }
}