using BusinessObject;
using DataAccess.DTO;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IPermissionRepository
{
    Task AddRangeAsync(List<Permission> permissions);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task AddAsync(Permission permission);
    Task<Permission?> FindPermissionByNameAsync(string permissionName);
    
}