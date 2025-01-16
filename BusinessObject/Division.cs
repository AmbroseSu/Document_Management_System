using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Division
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public bool IsDeleted { get; set; }
    
    public List<User>? Users { get; set; }
}