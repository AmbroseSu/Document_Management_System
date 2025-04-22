using BusinessObject;
using BusinessObject.Enums;


namespace DataAccess.DTO;

public class TaskDto
{
    public Guid TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TasksStatus? TaskStatus { get; set; }
    public TaskType TaskType { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int TaskNumber { get; set; }
    public bool? IsDeleted { get; set; }
    public bool? IsActive { get; set; }

    public Guid? StepId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? UserId { get; set; }
    public UserDto? User { get; set; }
}