using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class AttachmentDocumentRequest
{
    public string Name { get; set; }
    public string Url { get; set; } 
}