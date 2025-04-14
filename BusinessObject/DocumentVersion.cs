using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class DocumentVersion
{
    private DateTime _createDate;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentVersionId { get; set; }

    public string? VersionNumber { get; set; }
    public string? DocumentVersionUrl { get; set; }

    public DateTime CreateDate
    {
        get => _createDate.ToLocalTime();
        set => _createDate = value.ToUniversalTime();
    }

    public bool IsFinalVersion { get; set; }

    public Guid DocumentId { get; set; }
    [JsonIgnore]
    public Document? Document { get; set; }
    public List<DocumentSignature>? DocumentSignatures { get; set; }
}