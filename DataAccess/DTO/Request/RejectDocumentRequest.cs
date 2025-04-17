namespace DataAccess.DTO.Request;

public class RejectDocumentRequest
{
    public String Reason { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
}