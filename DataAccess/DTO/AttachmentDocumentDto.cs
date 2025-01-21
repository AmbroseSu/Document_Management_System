namespace DataAccess.DTO;

public class AttachmentDocumentDto
{
    public Guid AttachmentDocumentId { get; set; }
    public string? AttachmentDocumentName { get; set; }
    public string? AttachmentDocumentUrl { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid? DocumentId { get; set; }
    public Guid? DocumentFileExtensionId { get; set; }
}