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
    public string? ResourceMethod { get; set; }
    
    public List<ResourcePermission>? ResourcePermissions { get; set; }

    /*public Resource()
    {
        ResourceId = Guid.NewGuid();
    }*/
}