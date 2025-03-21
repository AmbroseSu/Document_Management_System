using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IRoleResourceRepository
{
    Task AddRangeAsync(List<RoleResource> roleResources);
    Task AddAsync(RoleResource roleResource);
    Task UpdateAsync(RoleResource roleResource);
    Task<RoleResource?> FindRoleResourceByIdAsync(Guid? roleResourceId);
    Task<IEnumerable<RoleResource>> FindRoleResourcesByRoleIdAsync(Guid? roleId);
}