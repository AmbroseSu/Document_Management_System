namespace DataAccess.DTO;

public class RoleDto
{
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime? CreatedDate { get; set; }

    public Guid? StepId { get; set; }
    public List<Guid>? TaskUserIds { get; set; }
    public List<Guid>? UserRoleIds { get; set; }
    public List<Guid>? RolePermissionIds { get; set; }
}