namespace DataAccess.DTO.Request;

public class UserFilterRequest
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public UserFilters Filters { get; set; } = new();
    public SortUserOptions Sort { get; set; } = new();
}