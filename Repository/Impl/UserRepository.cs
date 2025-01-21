using BusinessObject;
using DataAccess.DAO;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public class UserRepository
{
    public async Task DeleteAsync(Guid userId)
    {
        User? result = (await BaseDao<User>.Instance.FindAsync(u => u.UserId == userId && u.IsDeleted == false)).SingleOrDefault();
        if (result != null)
        {
            User updateResult = result;
            updateResult.IsDeleted = true;
            await BaseDao<User>.Instance.UpdateAsync(updateResult);
        }
    }
    
    
}