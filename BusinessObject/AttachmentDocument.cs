using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class AttachmentDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid AttachmentDocumentId { get; set; }

    public string? AttachmentDocumentName { get; set; }
    public string? AttachmentDocumentUrl { get; set; }
    public bool IsDeleted { get; set; }

    public Guid DocumentId { get; set; }

    [JsonIgnore]
    public Document? Document { get; set; }
    /*public AttachmentDocument()
    {
        AttachmentDocumentId = Guid.NewGuid();
    }*/
}