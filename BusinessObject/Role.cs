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
    
    public Guid? StepId { get; set; }
    public Step? Step { get; set; }
    
    public List<TaskUser>? TaskUsers { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<RoleResource>? RoleResources { get; set; }

    /*public Role()
    {
        RoleId = Guid.NewGuid();
    }*/
}