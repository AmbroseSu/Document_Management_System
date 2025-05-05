using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class UserRepository : IUserRepository
{
    private readonly BaseDao<User> _userDao;

    public UserRepository(DocumentManagementSystemDbContext context)
    {
        _userDao = new BaseDao<User>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(User entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _userDao.AddAsync(entity);
    }

    public async Task UpdateAsync(User entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _userDao.UpdateAsync(entity);
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        if (email == null) throw new ArgumentNullException(nameof(email));
        return await _userDao.FindByAsync(u => u.Email!.ToLower().Equals(email.ToLower()));
    }
    


    public async Task<User?> FindUserByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _userDao.FindByAsync(u => u.UserId == id, 
            ur => ur.Include(urr => urr.UserRoles).ThenInclude(r => r.Role)
            .Include(d => d.Division)
            .Include(t => t.Tasks)
            .Include(u => u.DigitalCertificates));
    }

    public async Task<User?> FindUserByUserNameAsync(string userName)
    {
        if (userName == null) throw new ArgumentNullException(nameof(userName));
        return await _userDao.FindByAsync(u => u.UserName!.ToLower().Equals(userName.ToLower()));
    }


    
    public async Task<IEnumerable<User>> FindAllUserAsync()
    {
        return await _userDao.FindAsync(u => true,
            u => u.Include(d => d.Division).Include(ur => ur.UserRoles).ThenInclude(r => r.Role));
    }

    public Task AddRangeAsync(List<User> users)
    {
        if (users == null) throw new ArgumentNullException(nameof(users));
        return _userDao.AddRangeAsync(users);
    }
}