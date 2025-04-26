namespace DataAccess.DTO.Request;

public class GrantDocumentRequest
{
    public Guid DocumentId { get; set; }
    public List<Guid> UserIds { get; set; }
}