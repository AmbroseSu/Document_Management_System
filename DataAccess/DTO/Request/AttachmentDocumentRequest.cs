using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class AttachmentDocumentRequest
{
    public string DocumentName { get; set; }
    public string urlfile { get; set; } 
}