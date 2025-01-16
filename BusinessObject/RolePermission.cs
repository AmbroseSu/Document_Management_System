using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class RolePermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int RolePermissionId { get; set; }
    
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
    
    public List<ResourcePermission>? ResourcePermissions { get; set; }
}