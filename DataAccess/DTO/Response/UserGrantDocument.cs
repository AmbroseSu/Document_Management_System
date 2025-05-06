using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class UserGrantDocument
{
    public Guid UserId { get; set; }
    public GrantPermission GrantPermission { get; set; }
}