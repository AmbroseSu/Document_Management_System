using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentSignature
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentSignatureId { get; set; }
    private DateTime _signedAt;
    public DateTime SignedAt {  
        get => _signedAt.ToLocalTime();  
        set => _signedAt = value.ToUniversalTime();  
    }
    private DateTime _validFrom;
    public DateTime ValidFrom {  
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