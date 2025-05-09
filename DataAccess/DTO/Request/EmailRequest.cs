using System.Text;
using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class EmailRequest
{
    //public String YourEmail { get; set; }
    public List<String> ReceiverEmail { get; set; }
    public List<String>? CcEmails { get; set; }
    public List<String>? BccEmails { get; set; }
    public String Subject { get; set; }
    public String Body { get; set; }
    public String AccessToken { get; set; }
    public Guid? DocumentId { get; set; }

    public override string ToString()
    {
        var result = new StringBuilder();

        result.AppendLine("=== Thông tin email ===");
        result.AppendLine($"Người nhận chính: {(ReceiverEmail != null && ReceiverEmail.Any() ? string.Join(", ", ReceiverEmail) : "Không có")}");
        result.AppendLine($"Người nhận Cc: {(CcEmails != null && CcEmails.Any() ? string.Join(", ", CcEmails) : "Không có")}");
        result.AppendLine($"Người nhận Bcc: {(BccEmails != null && BccEmails.Any() ? string.Join(", ", BccEmails) : "Không có")}");
        result.AppendLine($"Tiêu đề: {Subject}");
        result.AppendLine($"Nội dung: {Body}");
        result.AppendLine($"Access Token: {AccessToken}");
        result.AppendLine($"Document ID: {(DocumentId.HasValue ? DocumentId.ToString() : "Không có")}");

        return result.ToString();
    }
}