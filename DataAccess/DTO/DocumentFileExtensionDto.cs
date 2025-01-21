namespace DataAccess.DTO;

public class DocumentFileExtensionDto
{
    public Guid DocumentFileExtensionId { get; set; }
    public string? DocumentFileExtensionName { get; set; }
    public bool IsDeleted { get; set; }
    
    public List<Guid>? DocumentIds { get; set; }
    public List<Guid>? AttachmentDocumentIds { get; set; }
}