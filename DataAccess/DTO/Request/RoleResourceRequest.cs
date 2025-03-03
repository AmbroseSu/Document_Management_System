namespace DataAccess.DTO.Request;

public class RoleResourceRequest
{
    public Guid RoleId { get; set; }
    public List<Guid>? ResourceIds { get; set; }
}