namespace DataAccess.DTO.Request;

public class TaskRequest
{
    public Guid TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
}