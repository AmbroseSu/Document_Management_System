namespace DataAccess.DTO.Request;

public class VerifyOtpRequest
{
    public string Email { get; set; }
    public string OtpCode { get; set; }
}