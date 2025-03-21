using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Role
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime? CreatedDate { get; set; }
    
    
    public List<Step>? Steps { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<RoleResource>? RoleResources { get; set; }

    /*public Role()
    {
        RoleId = Guid.NewGuid();
    }*/
}