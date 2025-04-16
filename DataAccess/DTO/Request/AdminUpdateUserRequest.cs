using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class AdminUpdateUserRequest
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Position { get; set; }
    public Guid? DivisionId { get; set; }
    public string? Avatar { get; set; }
    public Guid? SubRoleId { get; set; }
}