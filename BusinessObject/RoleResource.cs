using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class RoleResource
{
    public Guid RoleResourceId { get; set; }
    
    public Guid RoleId { get; set; }
    public Guid ResourceId { get; set; }
    public Role? Role { get; set; }
    public Resource? Resource { get; set; }
    public bool IsDeleted { get; set; }
    

    /*public RolePermission()
    {
        RolePermissionId = Guid.NewGuid();
    }*/
}