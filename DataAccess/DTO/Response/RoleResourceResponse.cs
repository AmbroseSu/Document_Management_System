namespace DataAccess.DTO;

public class RoleResourceResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; }
    public List<PermissionDto> PermissionDtos { get; set; }
}