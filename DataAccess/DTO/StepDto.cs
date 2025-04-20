namespace DataAccess.DTO;

public class StepDto
{
    public Guid? StepId { get; set; }
    public int StepNumber { get; set; }
    public string? Action { get; set; }
    public Guid RoleId { get; set; }
    public RoleDto? Role { get; set; } 
    public Guid? NextStepId { get; set; }
    public Guid? RejectStepId { get; set; }
    public bool? IsFallbackStep { get; set; }
    public List<TaskDto>? TaskDtos { get; set; }
}