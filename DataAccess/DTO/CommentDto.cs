namespace DataAccess.DTO;

public class CommentDto
{
    public Guid CommentId { get; set; }
    public string? CommentContent { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsDeleted { get; set; }

    public Guid? UserId { get; set; }
    public Guid? TaskId { get; set; }
}