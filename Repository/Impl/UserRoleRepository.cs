using BusinessObject;
using DataAccess;
using DataAccess.DAO;
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

    public async Task<IEnumerable<UserRole>> FindRolesByUserIdAsync(Guid? userId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _userRoleDao.FindAsync(p => p.UserId == userId);
    }
}