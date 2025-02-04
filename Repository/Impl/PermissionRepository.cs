using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using DataAccess.DTO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class PermissionRepository : IPermissionRepository
{
    
    private readonly BaseDao<Permission> _permissionDao;
    
    public PermissionRepository(DocumentManagementSystemDbContext context)
    {
        _permissionDao = new BaseDao<Permission>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddRangeAsync(List<Permission> permissions)
    {
        if (permissions == null) throw new ArgumentNullException(nameof(permissions));
        await _permissionDao.AddRangeAsync(permissions);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _permissionDao.GetAllAsync();
    }

    public async Task AddAsync(Permission permission)
    {
        if (permission == null) throw new ArgumentNullException(nameof(permission));
        await _permissionDao.AddAsync(permission);
    }

    public async Task<Permission?> FindPermissionByNameAsync(string permissionName)
    {
        if (permissionName == null) throw new ArgumentNullException(nameof(permissionName));
        return await _permissionDao.FindByAsync(p => p.PermissionName == permissionName);
    }
    
    public async Task<Permission?> FindPermissionByIdAsync(Guid? permissionId)
    {
        if (permissionId == null) throw new ArgumentNullException(nameof(permissionId));
        return await _permissionDao.FindByAsync(p => p.PermissionId == permissionId);
    }
}