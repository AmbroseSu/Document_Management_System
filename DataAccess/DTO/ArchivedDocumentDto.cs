namespace DataAccess.DTO;

public class ArchivedDocumentDto
{
    public Guid ArchivedDocumentId { get; set; }
    public string? ArchivedDocumentName { get; set; }
    public string? ArchivedDocumentUrl { get; set; }
    public DateTime CreatedDate { get; set; }

    public Guid DocumentTypeId { get; set; }
    public List<Guid>? UserDocumentPermissionIds { get; set; }
    public List<Guid>? AttachmentArchivedDocumentIds { get; set; }
}