using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Permission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid PermissionId { get; set; }
    public string? PermissionName { get; set; }
    
    public List<Resource>? Resources { get; set; }


    /*public Permission()
    {
        PermissionId = Guid.NewGuid();
    }*/
}