namespace DataAccess.DTO;

public class RolePermissionDto
{
    public Guid RolePermissionId { get; set; }
    
    public Guid? RoleId { get; set; }
    public Guid? PermissionId { get; set; }
    public List<Guid>? ResourcePermissionIds { get; set; }
}