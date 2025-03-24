using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class ArchivedDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ArchivedDocumentId { get; set; }
    public string? ArchivedDocumentName { get; set; }
    public string? ArchivedDocumentContent { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SignedBy { get; set; }
    public string? ArchivedDocumentUrl { get; set; }
    private DateTime _createdDate;
    public DateTime CreatedDate {  
        get => _createdDate.ToLocalTime();  
        set => _createdDate = value.ToUniversalTime();  
    }
    public string? Sender { get; set; }
    public string? CreatedBy { get; set; }
    public string? ExternalPartner { get; set; }
    public ArchivedDocumentStatus ArchivedDocumentStatus { get; set; }
    private DateTime? _dateIssued;
    public DateTime? DateIssued {  
        get => _dateIssued?.ToLocalTime();  
        set => _dateIssued = value?.ToUniversalTime();  
    }
    private DateTime? _dateReceived;
    public DateTime? DateReceived {  
        get => _dateReceived?.ToLocalTime();  
        set => _dateReceived = value?.ToUniversalTime();  
    }
    private DateTime? _dateSented;
    public DateTime? DateSented {  
        get => _dateSented?.ToLocalTime();  
        set => _dateSented = value?.ToUniversalTime();  
    }
    public Guid? DocumentRevokeId { get; set; }
    public Guid? DocumentReplaceId { get; set; }
    public Scope Scope { get; set; }
    public bool IsTemplate { get; set; }
    
    public Guid DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public Guid FinalDocumentId { get; set; }
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