using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BusinessObject.Enums;

namespace BusinessObject;

public class Tasks
{
    private DateTime _createdDate;
    private DateTime _updatedDate;
    private DateTime _endDate;
    private DateTime _startDate;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid TaskId { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime StartDate
    {
        get => DateTime.SpecifyKind(_startDate, DateTimeKind.Utc).ToLocalTime();
        set => _startDate = value.ToUniversalTime();
    }

    public DateTime EndDate
    {
        get => DateTime.SpecifyKind(_endDate, DateTimeKind.Utc).ToLocalTime();
        set => _endDate = value.ToUniversalTime();
    }

    public TasksStatus TaskStatus { get; set; }
    public TaskType TaskType { get; set; }

    public DateTime CreatedDate
    {
        get => DateTime.SpecifyKind(_createdDate, DateTimeKind.Utc).ToLocalTime();
        set => _createdDate = value.ToUniversalTime();
    }
    
    public DateTime UpdatedDate
    {
        get => DateTime.SpecifyKind(_updatedDate, DateTimeKind.Utc).ToLocalTime();
        set => _updatedDate = value.ToUniversalTime();
    }
    public int TaskNumber { get; set; }
    public String CreatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Guid StepId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid UserId { get; set; }
    [JsonIgnore]
    public Step? Step { get; set; }
    [JsonIgnore]
    public Document? Document { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    

    /*public Task()
    {
        TaskId = Guid.NewGuid();
    }*/
}