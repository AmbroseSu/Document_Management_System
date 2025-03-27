namespace DataAccess.DTO;

public class MeatadataDto
{
    public MeatadataDto()
    {
    }

    public MeatadataDto(bool hasNextPage, bool hasPrevPage, int limit, int total, int page)
    {
        this.hasNextPage = hasNextPage;
        this.hasPrevPage = hasPrevPage;
        this.limit = limit;
        this.total = total;
        this.page = page;
    }

    public bool hasNextPage { get; set; }
    public bool hasPrevPage { get; set; }
    public int limit { get; set; }
    public int total { get; set; }
    public int page { get; set; }
}