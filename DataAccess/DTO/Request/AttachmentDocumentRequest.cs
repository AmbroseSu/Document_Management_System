using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class AttachmentDocumentRequest
{
    public string name { get; set; }
    public string url { get; set; } 
}