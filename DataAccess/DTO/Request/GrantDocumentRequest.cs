using DataAccess.DTO.Response;

namespace DataAccess.DTO.Request;

public class GrantDocumentRequest
{
    public Guid DocumentId { get; set; }
    public List<UserGrantDocument> UserGrantDocuments { get; set; }
}