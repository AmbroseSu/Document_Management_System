using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace BusinessObject;

public class Deadline
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int DeadlineId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReminderDate { get; set; }
    public bool IsDeleted { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public List<Document>? Documents { get; set; }
}