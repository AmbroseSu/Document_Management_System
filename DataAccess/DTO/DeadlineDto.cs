namespace DataAccess.DTO;

public class DeadlineDto
{
    public Guid DeadlineId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReminderDate { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid? UserId { get; set; }
    public Guid? DocumentId { get; set; }
}