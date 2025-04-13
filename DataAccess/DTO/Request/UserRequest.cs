using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class UserRequest
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string IdentityCard { get; set; }
    public Guid DivisionId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Position { get; set; }
    public string Address { get; set; }
    public Gender Gender { get; set; }
}