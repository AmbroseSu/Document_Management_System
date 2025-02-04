using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly BaseDao<RolePermission> _rolePermissionDao;

    public RolePermissionRepository(DocumentManagementSystemDbContext context)
    {
        _rolePermissionDao = new BaseDao<RolePermission>(context ?? throw new ArgumentNullException(nameof(context)));;
    }
    
    public async Task AddAsync(RolePermission rolePermission)
    {
        if (rolePermission == null) throw new ArgumentNullException(nameof(rolePermission));
        await _rolePermissionDao.AddAsync(rolePermission);
    }
}