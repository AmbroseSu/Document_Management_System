using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class DocumentTabResponse
{
    public DocumentDto? DocumentDto { get; set; }
    public String? WorkflowName { get; set; }
}