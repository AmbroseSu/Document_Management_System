using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class UserUpdateRequest
{
    public Guid UserId { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Avatar { get; set; }
}