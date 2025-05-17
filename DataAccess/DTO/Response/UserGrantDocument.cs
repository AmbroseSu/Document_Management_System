using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class UserGrantDocument
{
    public Guid UserId { get; set; }
    public GrantPermission GrantPermission { get; set; }

    public override string ToString()
    {
        return $"""
                User Id: {UserId},
                Grant Permission: {GrantPermission.ToString()}
                """;
    }
}