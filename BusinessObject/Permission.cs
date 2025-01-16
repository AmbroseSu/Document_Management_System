using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Permission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int PermissionId { get; set; }
    public string? PermissionName { get; set; }
    
    public List<RolePermission>? RolePermissions { get; set; }
    public List<UserDocumentPermission>? UserDocumentPermissions { get; set; }
}