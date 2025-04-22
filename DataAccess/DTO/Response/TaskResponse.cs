namespace DataAccess.DTO.Response;

public class TaskResponse
{
    public TaskDto TaskDto { get; set; }
    public String? FullName { get; set; }
    public String? Avatar { get; set; }
    public String? RoleName { get; set; }
}