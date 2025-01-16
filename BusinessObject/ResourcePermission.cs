using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class ResourcePermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ResourcePermissionId { get; set; }
    
    public Guid ResourceId { get; set; }
    public Guid RolePermissionId { get; set; }
    public Resource? Resource { get; set; }
    public RolePermission? RolePermission { get; set; }

    /*public ResourcePermission()
    {
        ResourcePermissionId = Guid.NewGuid();
    }*/
}