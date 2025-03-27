using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IUserRepository
{
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task<User?> FindUserByEmailAsync(string email);
    Task<User?> FindUserByIdAsync(Guid? id);
    Task<User?> FindUserByUserNameAsync(string userName);
    Task<IEnumerable<User>> FindAllUserAsync();
}