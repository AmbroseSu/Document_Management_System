using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Comment
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int CommentId { get; set; }
    public string? CommentContent { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsDeleted { get; set; }
    
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public User? User { get; set; }
    public Task? Task { get; set; }
}