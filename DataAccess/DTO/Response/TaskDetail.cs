using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class TaskDetail
{
    public TaskDto TaskDto { get; set; }
    public Scope Scope { get; set; }
    public string WorkflowName { get; set; }
    public Guid WorkflowId { get; set; }
    public string StepAction { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentTypeName { get; set; }
    public string UserNameCreateTask { get; set; }
}