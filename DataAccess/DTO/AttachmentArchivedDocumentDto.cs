namespace DataAccess.DTO;

public class AttachmentArchivedDocumentDto
{
    public Guid AttachmentArchivedDocumentId { get; set; }
    public string? AttachmentName { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid? ArchivedDocumentId { get; set; }
}