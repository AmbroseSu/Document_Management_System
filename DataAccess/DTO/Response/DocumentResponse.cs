using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class DocumentResponse
{
    public DocumentDto? DocumentDto { get; set; }
    public String? WorkflowName { get; set; }
}