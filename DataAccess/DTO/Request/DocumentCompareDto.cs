using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class DocumentCompareDto
{
    public Guid DocumentId { get; set; }
    public string DocumentName { get; set; }
    public string DocumentTypeName { get; set; }
    public string AiDocumentName { get; set; }
    public string AiDocumentType { get; set; }
    public string DocumentContent { get; set; }
    public string NumberOfDocument { get;set; }
    public bool IsDifferent { get; set; }
    public string FileBase64 { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime? Deadline { get; set; }
    
    public List<AttachmentDocumentRequest> Attachments { get; set; } = new List<AttachmentDocumentRequest>();
}