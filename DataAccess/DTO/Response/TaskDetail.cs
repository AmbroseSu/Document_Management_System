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
    public string DocumentName { get; set; }
    public string DocumentTypeName { get; set; }
    public String UserDoTask { get; set; }
    public string UserNameCreateTask { get; set; }
    public string FileSize { get; set; }
    public Boolean? IsUsb { get; set; } 
}