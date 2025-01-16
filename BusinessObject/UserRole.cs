using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class UserRole
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int UserRoleId { get; set; }
    public bool IsPrimary { get; set; }
    
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }
}