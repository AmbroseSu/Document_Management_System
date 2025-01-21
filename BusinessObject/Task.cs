using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace BusinessObject;

public class Task
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
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
    
    public Guid StepId { get; set; }
    public Guid DocumentId { get; set; }
    public Step? Step { get; set; }
    public Document? Document { get; set; }
    
    public List<TaskUser>? TaskUsers { get; set; }
    public List<Comment>? Comments { get; set; }

    /*public Task()
    {
        TaskId = Guid.NewGuid();
    }*/
}