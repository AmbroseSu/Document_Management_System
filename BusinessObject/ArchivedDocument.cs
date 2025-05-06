using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class ArchivedDocument
{
    private DateTime _createdDate;
    private DateTime? _dateIssued;
    private DateTime? _dateReceived;
    private DateTime? _dateSented;
    private DateTime _expirationDate;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ArchivedDocumentId { get; set; }

    public string? ArchivedDocumentName { get; set; }
    public string? ArchivedDocumentContent { get; set; }
    public string SystemNumberOfDoc { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SignedBy { get; set; }
    public DateTime ExpirationDate     
    {
        get => DateTime.SpecifyKind(_expirationDate, DateTimeKind.Utc).ToLocalTime();
        set => _expirationDate = value.ToUniversalTime();
    }
    public string? ArchivedDocumentUrl { get; set; }

    public DateTime CreatedDate
    {
        get => DateTime.SpecifyKind(_createdDate, DateTimeKind.Utc).ToLocalTime();
        set => _createdDate = value.ToUniversalTime();
    }

    public string? Sender { get; set; }
    public string? CreatedBy { get; set; }
    public string? ExternalPartner { get; set; }
    public ArchivedDocumentStatus ArchivedDocumentStatus { get; set; }

    public DateTime? DateIssued
    {
        get => _dateIssued.HasValue ? DateTime.SpecifyKind(_dateIssued.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _dateIssued = value?.ToUniversalTime();
    }

    public DateTime? DateReceived
    {
        get => _dateReceived.HasValue ? DateTime.SpecifyKind(_dateReceived.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _dateReceived = value?.ToUniversalTime();
    }

    public DateTime? DateSented
    {
        get => _dateSented.HasValue ? DateTime.SpecifyKind(_dateSented.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _dateSented = value?.ToUniversalTime();
    }

    public Guid? DocumentRevokeId { get; set; }
    public Guid? DocumentReplaceId { get; set; }
    public Scope? Scope { get; set; }
    public int? Llx { get; set; }
    public int? Lly { get; set; }
    public int? Urx { get; set; }
    public int? Ury { get; set; }
    public int? Page { get; set; }
    
    public bool IsTemplate { get; set; }

    public Guid DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public Guid? FinalDocumentId { get; set; }
    public Document? FinalDocument { get; set; }

    public List<UserDocumentPermission>? UserDocumentPermissions { get; set; }
    public List<AttachmentArchivedDocument>? AttachmentArchivedDocuments { get; set; }
    public List<ArchiveDocumentSignature>? ArchiveDocumentSignatures { get; set; }
    public List<ArchivedDocument>? DocumentReplaces { get; set; }
    public List<ArchivedDocument>? DocumentRevokes { get; set; }

    public List<Document>? CreateDocuments { get; set; }
    /*public ArchivedDocument()
    {
        ArchivedDocumentId = Guid.NewGuid();
    }*/
}