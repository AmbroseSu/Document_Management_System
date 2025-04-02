namespace DataAccess.DTO;

public class DivisionDto
{
    public Guid DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public bool? IsDeleted { get; set; }

    //public List<Guid>? UserIds { get; set; }
}