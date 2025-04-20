namespace DataAccess.DTO.Response;

public class TemplateResponseDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateDescription { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
}