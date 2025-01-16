using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class TaskUser
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int TaskUserId { get; set; }
    public bool IsCreatedTaskByUser { get; set; }
    
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public Task? Task { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }
    
}