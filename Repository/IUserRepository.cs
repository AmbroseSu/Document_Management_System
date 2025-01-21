using System.Linq.Expressions;
using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IUserRepository
{
    Task SaveAsync(User user);
    Task UpdateAsync(User user);
    public Task<IEnumerable<User?>> Find(Expression<Func<User, bool>> predicate);
    /*Task<User?> FindByEmailAsync(Expression<Func<User, bool>> predicate);
    Task<User?> FindByUserNameAsync(Expression<Func<User, bool>> predicate);*/
    //Task<User?> FindByIdAsync(Guid id);
    
}