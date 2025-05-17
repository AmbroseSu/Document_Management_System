using DataAccess.DTO.Response;

namespace DataAccess.DTO.Request;

public class GrantDocumentRequest
{
    public Guid DocumentId { get; set; }
    public List<UserGrantDocument> UserGrantDocuments { get; set; }
    
    public override string ToString()
    {
        return $"""
                Document Id: {DocumentId},
                User Grant Documents: {string.Join(", ", UserGrantDocuments)}
                """;
    }
}