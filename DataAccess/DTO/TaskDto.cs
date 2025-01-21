using BusinessObject.Enums;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace DataAccess.DTO;

public class TaskDto
{
    public Guid TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TaskStatus TaskStatus { get; set; }
    public TaskType TaskType { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    
    public Guid? StepId { get; set; }
    public Guid? DocumentId { get; set; }
    public List<Guid>? TaskUserIds { get; set; }
    public List<Guid>? CommentIds { get; set; }
}