using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class RolePermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid RolePermissionId { get; set; }
    
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
    
    public List<ResourcePermission>? ResourcePermissions { get; set; }

    /*public RolePermission()
    {
        RolePermissionId = Guid.NewGuid();
    }*/
}