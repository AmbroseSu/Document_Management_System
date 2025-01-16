using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class UserDocumentPermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int UserDocumentPermissionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDeleted { get; set; }
    
    public int PermissionId { get; set; }
    public int UserId { get; set; }
    public int ArchivedDocumentId { get; set; }
    public Permission? Permission { get; set; }
    public User? User { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }
}