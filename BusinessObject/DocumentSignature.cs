using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentSignature
{
    private DateTime _signedAt;
    private DateTime _validFrom;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentSignatureId { get; set; }

    public DateTime SignedAt
    {
        get => _signedAt.ToLocalTime();
        set => _signedAt = value.ToUniversalTime();
    }

    public DateTime ValidFrom
    {
        get => _validFrom.ToLocalTime();
        set => _validFrom = value.ToUniversalTime();
    }

    public int OrderIndex { get; set; }
    public string SignatureValue { get; set; }

    public Guid DigitalCertificateId { get; set; }
    public DigitalCertificate? DigitalCertificate { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
}