using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DigitalCertificate
{
    private DateTime _validFrom;
    private DateTime _validTo;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DigitalCertificateId { get; set; }

    public string? SerialNumber { get; set; }
    public string? Issuer { get; set; }

    public DateTime ValidFrom
    {
        get => DateTime.SpecifyKind(_validFrom, DateTimeKind.Utc).ToLocalTime();
        set => _validFrom = value.ToUniversalTime();
    }

    public DateTime ValidTo
    {
        get => DateTime.SpecifyKind(_validTo, DateTimeKind.Utc).ToLocalTime();
        set => _validTo = value.ToUniversalTime();
    }
    
    public string? Subject { get; set; }
    public Boolean? IsUsb { get; set; }
    public string? SignatureImageUrl { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public List<ArchiveDocumentSignature>? ArchiveDocumentSignatures { get; set; }
    public List<DocumentSignature>? DocumentSignatures { get; set; }
}