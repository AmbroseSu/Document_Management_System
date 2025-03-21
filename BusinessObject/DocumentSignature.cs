using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentSignature
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentSignatureId { get; set; }
    public DateTime SignedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public int OrderIndex { get; set; }
    public string SignatureValue { get; set; }
    
    public Guid DigitalCertificateId { get; set; }
    public DigitalCertificate? DigitalCertificate { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
}