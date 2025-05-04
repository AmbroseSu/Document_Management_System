using BusinessObject.Enums;

namespace DataAccess.DTO.Request;

public class GetAllArchiveRequestDto
{
    public string? Name { get; set; }
    public Scope? Scope { get; set; }
    public DateTime? CreatedDate { get; set; }
    public ArchivedDocumentStatus? Status { get; set; }
}