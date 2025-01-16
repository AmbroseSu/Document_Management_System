using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Role
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public List<Step>? Steps { get; set; }
    public List<TaskUser>? TaskUsers { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<RolePermission>? RolePermissions { get; set; }
}