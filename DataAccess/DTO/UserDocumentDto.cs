namespace DataAccess.DTO;

public class UserDocumentDto
{
    public Guid UserDocumentId { get; set; }
    public bool IsCreatedDocumentByUser { get; set; }

    public Guid? DocumentId { get; set; }
    public Guid? UserId { get; set; }
}