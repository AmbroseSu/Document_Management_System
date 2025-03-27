using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IRoleRepository
{
    Task AddRangeAsync(List<Role> roles);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<IEnumerable<Role>> GetAllActiveAsync();
    Task<IEnumerable<Role>> GetAllByMainRoleAsync();
    Task<IEnumerable<Role>> GetAllBySubRoleAsync();
    Task AddAsync(Role role);
    Task<Role?> FindRoleByNameAsync(string roleName);
    Task<Role?> FindRoleByIdAsync(Guid? roleId);
    
}