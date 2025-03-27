namespace DataAccess.DTO;

public class ResourceResponse
{
    public Guid ResourceId { get; set; }
    public string ResourceApi { get; set; }
    public string ResourceName { get; set; }
    public bool IsDeleted { get; set; }
    
}