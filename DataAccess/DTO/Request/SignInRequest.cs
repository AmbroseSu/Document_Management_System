namespace DataAccess.DTO.Request;

public class SignInRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FcmToken { get; set; }
}