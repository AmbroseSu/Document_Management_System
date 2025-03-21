using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class DocumentVersion
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentVersionId { get; set; }
    public string? VersionNumber { get; set; }
    public string? DocumentVersionUrl { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsFinalVersion { get; set; }
    
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
}