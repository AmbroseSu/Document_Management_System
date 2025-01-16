using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class ResourcePermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int ResourcePermissionId { get; set; }
    
    public int ResourceId { get; set; }
    public int RolePermissionId { get; set; }
    public Resource? Resource { get; set; }
    public RolePermission? RolePermission { get; set; }
}