namespace DataAccess.DTO;

public class ResourcePermissionDto
{
    public Guid ResourcePermissionId { get; set; }
    
    public Guid? ResourceId { get; set; }
    public Guid? RolePermissionId { get; set; }
}