namespace DataAccess.DTO.Request;

public class VersionOfDocResponse
{
    public Guid VersionId { get; set; }
    public String VersionNumber { get; set; }
    public DateTime DateReject { get; set; }
    public Guid UserIdReject { get; set; }
    public String UserReject { get; set; }
}