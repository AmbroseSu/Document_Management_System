namespace DataAccess.DTO;

public class TaskUserDto
{
    public Guid TaskUserId { get; set; }
    public bool IsCreatedTaskByUser { get; set; }
    
    public Guid? TaskId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
}