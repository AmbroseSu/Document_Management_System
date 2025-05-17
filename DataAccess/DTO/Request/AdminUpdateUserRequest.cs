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

    public override string ToString()
    {
        return $"""
                UserId: {UserId}
                FullName: {FullName ?? "N/A"}
                Email: {Email ?? "N/A"}
                Address: {Address ?? "N/A"}
                PhoneNumber: {PhoneNumber ?? "N/A"}
                Gender: {Gender?.ToString() ?? "N/A"}
                DateOfBirth: {(DateOfBirth.HasValue ? DateOfBirth.Value.ToString("yyyy-MM-dd") : "N/A")}
                Position: {Position ?? "N/A"}
                DivisionId: {DivisionId?.ToString() ?? "N/A"}
                Avatar: {Avatar ?? "N/A"}
                SubRoleId: {SubRoleId?.ToString() ?? "N/A"}
                """;
    }
}