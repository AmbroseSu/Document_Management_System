using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class UserDocumentPermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserDocumentPermissionId { get; set; }
    private DateTime _createdDate;
    public DateTime CreatedDate {  
        get => _createdDate.ToLocalTime();  
        set => _createdDate = value.ToUniversalTime();  
    }
    public bool IsDeleted { get; set; }
    

    public Guid UserId { get; set; }
    public Guid ArchivedDocumentId { get; set; }
    public User? User { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }

    /*public UserDocumentPermission()
    {
        UserDocumentPermissionId = Guid.NewGuid();
    }*/
}