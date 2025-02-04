/*using BusinessObject;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO;

public class UserDao : BaseDao<User>
{
    private readonly DocumentManagementSystemDbContext _context;
    public UserDao(DocumentManagementSystemDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> FindUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> FindUserByUserNameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));

        return await _context.Set<User>().FirstOrDefaultAsync(u => u.UserName == userName);
    }
}*/