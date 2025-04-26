using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class DocumentUpload
{
    public Guid DocumentId { get; set; }
    public IFormFile File { get; set; }
}