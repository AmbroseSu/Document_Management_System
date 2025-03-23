using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IUserRepository
{
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task<User?> FindUserByEmail(string email);
    Task<User?> FindUserByUserName(string userName);
}