using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class ArchiveDocumentRequest
{
    public string Name {get; set;}
    public string Number {get; set;}
    public IFormFile Ima {get; set;}
}