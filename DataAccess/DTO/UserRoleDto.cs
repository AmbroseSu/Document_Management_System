namespace DataAccess.DTO;

public class UserRoleDto
{
    public Guid UserRoleId { get; set; }
    public bool IsPrimary { get; set; }

    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
}