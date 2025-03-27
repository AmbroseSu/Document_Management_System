using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class RoleResourceRepository : IRoleResourceRepository
{
    private readonly BaseDao<RoleResource> _roleResourceDao;

    public RoleResourceRepository(DocumentManagementSystemDbContext context)
    {
        _roleResourceDao = new BaseDao<RoleResource>(context ?? throw new ArgumentNullException(nameof(context)));
    }


    public async Task AddRangeAsync(List<RoleResource> roleResources)
    {
        if (roleResources == null) throw new ArgumentNullException(nameof(roleResources));
        await _roleResourceDao.AddRangeAsync(roleResources);
    }

    public async Task AddAsync(RoleResource roleResource)
    {
        if (roleResource == null) throw new ArgumentNullException(nameof(roleResource));
        await _roleResourceDao.AddAsync(roleResource);
    }

    public async Task UpdateAsync(RoleResource roleResource)
    {
        if (roleResource == null) throw new ArgumentNullException(nameof(roleResource));
        await _roleResourceDao.UpdateAsync(roleResource);
    }
    
    public async Task<IEnumerable<RoleResource>> GetAllAsync()
    {
        return await _roleResourceDao.GetAllAsync();
    }

    public async Task<RoleResource?> FindRoleResourceByIdAsync(Guid? roleResourceId)
    {
        if (roleResourceId == null) throw new ArgumentNullException(nameof(roleResourceId));
        return await _roleResourceDao.FindByAsync(rr => rr.RoleResourceId == roleResourceId);
    }

    public async Task<IEnumerable<RoleResource>> FindRoleResourcesByRoleIdAsync(Guid? roleId)
    {
        if (roleId == null) throw new ArgumentNullException(nameof(roleId));
        return await _roleResourceDao.FindAsync(p => p.RoleId == roleId && p.IsDeleted == false, p => p.Include(r => r.Resource));
    }
    
    // public async Task<IEnumerable<RoleResource>> FindAllRoleResourcesByRoleIdAsync(Guid? roleId, Guid? permissionId)
    // {
    //     if (roleId == null || permissionId == null) throw new ArgumentNullException(nameof(roleId), nameof(permissionId));
    //     return await _roleResourceDao.FindAsync(p => p.RoleId == roleId && p.Resource!.PermissionId == permissionId, p => p.Include(r => r.Resource));
    // }
    
    public async Task<IEnumerable<RoleResource>> FindAllRoleResourcesByRoleIdsAsync(List<Guid> roleIds)
    {
        if (roleIds == null || !roleIds.Any()) return Enumerable.Empty<RoleResource>();

        return await _roleResourceDao.FindAsync(
            p => roleIds.Contains(p.RoleId),
            p => p.Include(r => r.Resource)
        );
    }
}