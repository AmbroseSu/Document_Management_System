using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class GetAllMySelfRequestDto
{
    public string? Name { get; set; }
    public Scope? Scope { get; set; }
    public DateTime? StartCreatedDate { get; set; }
    public DateTime? EndCreatedDate { get; set; }

    public ProcessingStatus? Status { get; set; }

    public SortByCreatedDate SortByCreatedDate { get; set; } = SortByCreatedDate.Descending;
}
