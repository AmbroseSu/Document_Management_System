using BusinessObject;
using Repository.Impl;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IRolePermissionRepository
{
    Task AddAsync(RolePermission rolePermission);
}