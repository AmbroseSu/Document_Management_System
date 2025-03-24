namespace DataAccess.DTO;

public class ResponseDto
{
    public Object Content { get; set; }
    public string Message { get; set; }
    public int? Size { get; set; }
    public int StatusCode { get; set; }
    public MeatadataDto MeatadataDto { get; set; }
}