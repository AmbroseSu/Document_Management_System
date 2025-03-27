namespace DataAccess.DTO;

public class ResourceDto
{
    public Guid? ResourceId { get; set; }
    public string? ResourceApi { get; set; }
    public string? ResourceName { get; set; }
    public Guid PermissionId { get; set; }

    public List<Guid>? RoleResourceIds { get; set; }
}