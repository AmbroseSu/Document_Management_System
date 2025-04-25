namespace DataAccess.DTO;

public class UserDocumentPermissionDto
{
    public Guid? UserDocumentPermissionId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ArchivedDocumentId { get; set; }
}