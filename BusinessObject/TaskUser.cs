using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class TaskUser
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid TaskUserId { get; set; }
    public int? Order { get; set; }
    public string? RejectReason { get; set; }
    public TaskUserStatus TaskUserStatus { get; set; }
    public bool IsCreatedTaskByUser { get; set; }
    
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Task? Task { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }

    /*public TaskUser()
    {
        TaskUserId = Guid.NewGuid();
    }*/
}