using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace BusinessObject;

public class Deadline
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DeadlineId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReminderDate { get; set; }
    public bool IsDeleted { get; set; }
    
    
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }

    /*public Deadline()
    {
        DeadlineId = Guid.NewGuid();
    }*/
}