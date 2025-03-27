using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Resource
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid ResourceId { get; set; }

    public string? ResourceApi { get; set; }
    public string? ResourceName { get; set; }

    public Guid PermissionId { get; set; }
    public Permission? Permission { get; set; }

    public List<RoleResource>? RoleResources { get; set; }


    /*public Resource()
    {
        ResourceId = Guid.NewGuid();
    }*/
}