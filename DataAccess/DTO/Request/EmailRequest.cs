using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class EmailRequest
{
    public string YourEmail { get; set; }
    public string ReceiverEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string AccessToken { get; set; }
    public IFormFile? FilePath { get; set; }
}