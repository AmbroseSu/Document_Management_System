using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IUserRoleRepository
{
    Task AddAsync(UserRole role);
    Task<IEnumerable<UserRole>> FindRolesByUserIdAsync(Guid? userId);
    Task<IEnumerable<UserRole>> FindUserRolesByUserIdsAsync(List<Guid> userIds);
}