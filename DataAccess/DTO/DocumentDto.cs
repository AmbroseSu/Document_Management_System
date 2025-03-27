using BusinessObject.Enums;

namespace DataAccess.DTO;

public class DocumentDto
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public bool IsTemplate { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Guid? DocumentTypeId { get; set; }
    public Guid? DeadlineId { get; set; }
    public Guid? DocumentFileExtensionId { get; set; }
    public List<Guid>? TaskIds { get; set; }
    public List<Guid>? UserDocumentIds { get; set; }
    public List<Guid>? AttachmentDocumentIds { get; set; }
}