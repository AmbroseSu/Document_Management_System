using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class UserDocument
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserDocumentId { get; set; }
    public bool IsCreatedDocumentByUser { get; set; }
    
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public Document? Document { get; set; }
    public User? User { get; set; }

    /*public UserDocument()
    {
        UserDocumentId = Guid.NewGuid();
    }*/
}