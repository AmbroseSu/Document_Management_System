namespace DataAccess.DTO.Request;

public class RoleResourceRequest
{
    public Guid RoleId { get; set; }
    public List<Guid>? ResourceIds { get; set; }

    public override string ToString()
    {
        return $"RoleId: {RoleId}, ResourceIds: {string.Join(", ", ResourceIds)}";
    }
}