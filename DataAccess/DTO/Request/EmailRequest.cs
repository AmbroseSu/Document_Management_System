using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class EmailRequest
{
    public String YourEmail { get; set; }
    public String ReceiverEmail { get; set; }
    public List<String> CcEmails { get; set; }
    public List<String> BccEmails { get; set; }
    public String Subject { get; set; }
    public String Body { get; set; }
    public String AccessToken { get; set; }
    public Guid? DocumentId { get; set; }
}