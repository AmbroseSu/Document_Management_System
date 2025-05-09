using System.Text.Json.Serialization;
using BusinessObject.Enums;

namespace BusinessObject.Option;

public class DocumentElastic
{
    private DateTime _createdDate;
    private DateTime _expirationDate;
    protected bool Equals(Document other)
    {
        return DocumentId.Equals(other.DocumentId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Document)obj);
    }

    public override int GetHashCode()
    {
        return DocumentId.GetHashCode();
    }

    private DateTime _updatedDate;
    private DateTime _deadline;
    private DateTime? _dateReceived;
    private DateTime? _dateIssued;


    public Guid DocumentId { get; set; }

    public string? DocumentName { get; set; }
    public string? DocumentContent { get; set; }
    public string? NumberOfDocument { get; set; }
    public string SystemNumberOfDoc { get; set; }
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
    
    public DateTime ExpirationDate     
    {
        get => DateTime.SpecifyKind(_expirationDate, DateTimeKind.Utc).ToLocalTime();
        set => _expirationDate = value.ToUniversalTime();
    }
    public bool IsDeleted { get; set; }

    public Guid UserId { get; set; }

    public Guid? TemplateArchiveDocumentId { get; set; }

    public Guid? FinalArchiveDocumentId { get; set; }

    public Guid? DocumentTypeId { get; set; }
    
}