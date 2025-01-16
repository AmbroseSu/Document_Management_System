using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Resource
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int ResourceId { get; set; }
    public string? ResourceApi { get; set; }
    public string? ResourceName { get; set; }
    
    public List<ResourcePermission>? ResourcePermissions { get; set; }
    
}