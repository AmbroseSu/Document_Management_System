using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IUserRoleRepository
{
    Task AddAsync(UserRole role);
    Task DeleteAsync(UserRole userRole);
    Task<IEnumerable<UserRole>> FindRolesByUserIdAsync(Guid? userId);
    Task<IEnumerable<UserRole>> FindUserRolesByUserIdsAsync(List<Guid> userIds);
}