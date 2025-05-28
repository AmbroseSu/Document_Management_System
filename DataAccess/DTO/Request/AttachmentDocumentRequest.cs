using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class AttachmentDocumentRequest
{
    public string DocumentName { get; set; }
    public IFormFile file { get; set; } 
}