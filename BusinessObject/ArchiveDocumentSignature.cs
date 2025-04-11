using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class ArchiveDocumentSignature
{
    private DateTime _signedAt;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ArchiveDocumentSignatureId { get; set; }

    public DateTime SignedAt
    {
        get => _signedAt.ToLocalTime();
        set => _signedAt = value.ToUniversalTime();
    }
    

    public int OrderIndex { get; set; }

    public Guid DigitalCertificateId { get; set; }
    public DigitalCertificate? DigitalCertificate { get; set; }
    public Guid ArchivedDocumentId { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }
}