using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentSignature
{
    private DateTime _signedAt;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentSignatureId { get; set; }

    public DateTime SignedAt
    {
        get => DateTime.SpecifyKind(_signedAt, DateTimeKind.Utc).ToLocalTime();
        set => _signedAt = value.ToUniversalTime();
    }
    

    public int OrderIndex { get; set; }

    public Guid DigitalCertificateId { get; set; }
    public DigitalCertificate? DigitalCertificate { get; set; }
    public Guid DocumentVersionId { get; set; }
    public DocumentVersion? DocumentVersion { get; set; }
    
}