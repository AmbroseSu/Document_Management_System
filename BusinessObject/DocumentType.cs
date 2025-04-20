using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class DocumentType
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentTypeId { get; set; }

    public string? DocumentTypeName { get; set; }
    private DateTime _createAt;
    public DateTime CreateAt
    {
        get => DateTime.SpecifyKind(_createAt, DateTimeKind.Utc).ToLocalTime();
        set => _createAt = value.ToUniversalTime();
    }
    
    public string Acronym { get; set; }
    public Guid? CreateBy { get; set; }
    public bool IsDeleted { get; set; }

    public List<DocumentTypeWorkflow>? DocumentTypeWorkflows { get; set; }
    public List<ArchivedDocument>? ArchivedDocuments { get; set; }
    [JsonIgnore]
    public List<Document>? Documents { get; set; }

    /*public DocumentType()
    {
        DocumentTypeId = Guid.NewGuid();
    }*/
}