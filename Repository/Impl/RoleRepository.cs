using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class RoleRepository : IRoleRepository
{
    private readonly BaseDao<Role> _roleDao;

    public RoleRepository(DocumentManagementSystemDbContext context)
    {
        _roleDao = new BaseDao<Role>(context ?? throw new ArgumentNullException(nameof(context)));
    }


    public async Task AddRangeAsync(List<Role> roles)
    {
        if (roles == null) throw new ArgumentNullException(nameof(roles));
        await _roleDao.AddRangeAsync(roles);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _roleDao.GetAllAsync();
    }

    public async Task AddAsync(Role role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        await _roleDao.AddAsync(role);
    }

    public async Task<Role?> FindRoleByNameAsync(string roleName)
    {
        if (roleName == null) throw new ArgumentNullException(nameof(roleName));
        return await _roleDao.FindByAsync(p => p.RoleName == roleName);
    }
    
    public async Task<Role?> FindRoleByIdAsync(Guid? roleId)
    {
        if (roleId == null) throw new ArgumentNullException(nameof(roleId));
        return await _roleDao.FindByAsync(p => p.RoleId == roleId);
    }
    
    
}