namespace DataAccess.DTO.Request;

public class SortUserOptions
{
    public string Field { get; set; } = "CreatedAt";
    public string Order { get; set; } = "desc"; // "asc" hoặc "desc"
}