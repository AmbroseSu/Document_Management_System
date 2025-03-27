namespace DataAccess.DTO;

public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public string? PermissionName { get; set; }

    public List<ResourceResponse>? ResourceResponses { get; set; }
}