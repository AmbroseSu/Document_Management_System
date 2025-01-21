namespace DataAccess.DTO;

public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public string? PermissionName { get; set; }
    
    public List<Guid>? RolePermissionIds { get; set; }
    public List<Guid>? UserDocumentPermissionIds { get; set; }
}