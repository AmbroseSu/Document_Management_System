using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly BaseDao<UserRole> _userRoleDao;

    public UserRoleRepository(DocumentManagementSystemDbContext context)
    {
        _userRoleDao = new BaseDao<UserRole>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(UserRole userRole)
    {
        if (userRole == null) throw new ArgumentNullException(nameof(userRole));
        await _userRoleDao.AddAsync(userRole);
    }
    
    public async Task DeleteAsync(UserRole userRole)
    {
        if (userRole == null) throw new ArgumentNullException(nameof(userRole));
        await _userRoleDao.DeleteAsync(userRole);
    }

    public async Task<IEnumerable<UserRole>> FindRolesByUserIdAsync(Guid? userId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _userRoleDao.FindAsync(p => p.UserId == userId, r => r.Include(r => r.Role).Include(u => u.User));
    }
    
    public async Task<IEnumerable<UserRole>> FindUserRolesByUserIdsAsync(List<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return new List<UserRole>();

        return await _userRoleDao.FindAsync(
            ur => userIds.Contains(ur.UserId),
            q => q.Include(ur => ur.Role)
        );
    }
    
    public async Task<IEnumerable<UserRole>> FindUserRolesMainByUserIdsAsync(List<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return new List<UserRole>();

        return await _userRoleDao.FindAsync(
            ur => userIds.Contains(ur.UserId) && ur.IsPrimary == true,
            q => q.Include(ur => ur.Role)
        );
    }
    
    
}