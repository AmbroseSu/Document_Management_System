namespace DataAccess.DTO;

public class RoleResourceDto
{
    public Guid RoleResourceId { get; set; }
    
    public Guid RoleId { get; set; }
    public Guid ResourceId { get; set; }
    public bool IsDeleted { get; set; }
    
}