namespace DataAccess.DTO;

public class ResourceDto
{
    public Guid ResourceId { get; set; }
    public string? ResourceApi { get; set; }
    public string? ResourceName { get; set; }
    public string? ResourceMethod {get; set;}
    
    public List<Guid>? ResourcePermissionIds { get; set; }
}