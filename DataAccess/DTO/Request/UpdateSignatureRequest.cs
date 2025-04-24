using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class UpdateSignatureRequest
{
    public string Name { get; set; }
    public bool? IsDigital { get; set; }
    public IFormFile File { get; set; }
}