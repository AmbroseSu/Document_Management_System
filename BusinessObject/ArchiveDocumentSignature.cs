using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class ArchiveDocumentSignature
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ArchiveDocumentSignatureId { get; set; }
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
    public Guid ArchivedDocumentId { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }
    
}