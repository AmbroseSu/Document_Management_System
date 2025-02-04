using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IRoleRepository
{
    Task AddRangeAsync(List<Role> roles);
    Task<IEnumerable<Role>> GetAllAsync();
    Task AddAsync(Role role);
    Task<Role?> FindRoleByNameAsync(string roleName);
    Task<Role?> FindRoleByIdAsync(Guid? roleId);
}