using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class ArchiveDocumentSignature
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ArchiveDocumentSignatureId { get; set; }
    public DateTime SignedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public int OrderIndex { get; set; }
    public string SignatureValue { get; set; }
    
    public Guid DigitalCertificateId { get; set; }
    public DigitalCertificate? DigitalCertificate { get; set; }
    public Guid ArchivedDocumentId { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }
    
}