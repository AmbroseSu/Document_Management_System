using BusinessObject;
using DataAccess;
using DataAccess.DAO;
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

    public async Task<RoleResource?> FindRoleResourceByIdAsync(Guid? roleResourceId)
    {
        if (roleResourceId == null) throw new ArgumentNullException(nameof(roleResourceId));
        return await _roleResourceDao.FindByAsync(rr => rr.RoleResourceId == roleResourceId);
    }

    public async Task<IEnumerable<RoleResource>> FindRoleResourcesByRoleIdAsync(Guid? roleId)
    {
        if (roleId == null) throw new ArgumentNullException(nameof(roleId));
        return await _roleResourceDao.FindAsync(p => p.RoleId == roleId && p.IsDeleted == false);
    }
}