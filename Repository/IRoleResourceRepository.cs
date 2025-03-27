using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IRoleResourceRepository
{
    Task AddRangeAsync(List<RoleResource> roleResources);
    Task AddAsync(RoleResource roleResource);
    Task UpdateAsync(RoleResource roleResource);
    Task<IEnumerable<RoleResource>> GetAllAsync();
    Task<RoleResource?> FindRoleResourceByIdAsync(Guid? roleResourceId);
    Task<IEnumerable<RoleResource>> FindRoleResourcesByRoleIdAsync(Guid? roleId);
    //Task<IEnumerable<RoleResource>> FindAllRoleResourcesByRoleIdAsync(Guid? roleId, Guid? permissionId);
    Task<IEnumerable<RoleResource>> FindAllRoleResourcesByRoleIdsAsync(List<Guid> roleIds);
}