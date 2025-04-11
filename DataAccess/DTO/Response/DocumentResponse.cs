using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class DocumentResponse
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? NumberOfDocument { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime Deadline { get; set; }
    public string? SignedBy { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public string? FinalVersionUrl { get; set; } 
}