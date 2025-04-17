using DataAccess.DTO.Request;

namespace DataAccess.DTO.Response;

public class DocumentRejectResponse
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentType { get; set; }
    public string? WorkflowName { get; set; }
    public List<VersionOfDocResponse>? VersionOfDocResponses { get; set; }
}