using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class UserRole
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserRoleId { get; set; }

    public bool IsPrimary { get; set; }

    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    [JsonIgnore]
    public Role? Role { get; set; }

    /*public UserRole()
    {
        UserRoleId = Guid.NewGuid();
    }*/
}