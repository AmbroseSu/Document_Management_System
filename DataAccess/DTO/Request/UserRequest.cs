using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class UserRequest
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    //public string Password { get; set; }
    //public string Phonenumber { get; set; }
    public string Address { get; set; }
    public Gender Gender { get; set; }
}