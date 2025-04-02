using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace BusinessObject;

public class Division
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DivisionId { get; set; }

    public string? DivisionName { get; set; }
    private DateTime _createAt;
    public DateTime CreateAt
    {
        get => _createAt.ToLocalTime();
        set => _createAt = value.ToUniversalTime();
    }
    public Guid? CreateBy { get; set; }
    public bool IsDeleted { get; set; }

    public List<User>? Users { get; set; }

    /*public Division()
    {
        DivisionId = Guid.NewGuid();
    }*/
}