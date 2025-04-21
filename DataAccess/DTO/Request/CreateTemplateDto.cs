using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class CreateTemplateDto
{
    public string Name { get; set; }
    public int? Llx { get; set; }
    public int? Lly { get; set; }
    public int? Urx { get; set; }
    public int? Ury { get; set; }
    public int? Page { get; set; }
    public IFormFile TemplateFile { get; set; }
}