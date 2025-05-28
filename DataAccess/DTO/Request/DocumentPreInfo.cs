namespace DataAccess.DTO.Request;

public class DocumentPreInfo
{
    public Guid TemplateId { get; set; }
    public Guid WorkFlowId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public DateTime ExpireDate { get; set; }
    
    public DateTime? IssueDate { get; set; }
}