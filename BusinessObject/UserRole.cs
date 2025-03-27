using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class UserRole
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserRoleId { get; set; }

    public bool IsPrimary { get; set; }

    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }

    /*public UserRole()
    {
        UserRoleId = Guid.NewGuid();
    }*/
}