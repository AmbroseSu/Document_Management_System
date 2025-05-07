namespace DataAccess.DTO;

public class DocumentUploadDto
{

    public Dictionary<string, object> CanChange { get; set; } = new();
    public Dictionary<string, object> CannotChange { get; set; } = new();
    
}