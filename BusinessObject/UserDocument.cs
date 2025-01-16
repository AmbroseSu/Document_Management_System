using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class UserDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int UserDocumentId { get; set; }
    public bool IsCreatedDocumentByUser { get; set; }
    
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    public Document? Document { get; set; }
    public User? User { get; set; }
    
}