namespace DataAccess.DTO.Request;

public class SortUserOptions
{
    public string Field { get; set; } = "createdAt";
    public string Order { get; set; } = "desc"; // "asc" hoặc "desc"
}