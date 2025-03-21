using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DigitalCertificate
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DigitalCertificateId { get; set; }
    public string? SerialNumber { get; set; }
    public string? Issuer { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string? PublicKey { get; set; }
    public string? HashAlgorithm { get; set; }
    public string? OwnerName { get; set; }
    public bool IsRevoked { get; set; }
    public string? SignatureImageUrl { get; set; }
    
    public Guid UserId { get; set; }    
    public User? User { get; set; }
    
    public List<ArchiveDocumentSignature>? ArchiveDocumentSignatures { get; set; }
    public List<DocumentSignature>? DocumentSignatures { get; set; }
    
}