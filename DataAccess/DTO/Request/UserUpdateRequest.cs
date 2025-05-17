using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class UserUpdateRequest
{
    public Guid UserId { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Avatar { get; set; }

    public override string ToString()
    {
        return $"""
                UserId: {UserId},
                Address: {Address ?? "N/A"},
                Date Of Birth: {DateOfBirth},
                Gender: {Gender.ToString()}
                Avatar: {Avatar ?? "N/A"}
               """;
    }
}