using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObject;

public class Role
{
    private DateTime? _createdDate;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid RoleId { get; set; }

    public string? RoleName { get; set; }

    public DateTime? CreatedDate
    {
        get => _createdDate.HasValue ? DateTime.SpecifyKind(_createdDate.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _createdDate = value?.ToUniversalTime();
    }
    
    public bool IsDeleted { get; set; }


    [JsonIgnore]
    public List<Step>? Steps { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<RoleResource>? RoleResources { get; set; }

    /*public Role()
    {
        RoleId = Guid.NewGuid();
    }*/
}