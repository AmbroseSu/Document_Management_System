using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IDivisionRepository
{
    Task AddAsync(Division entity);
    Task UpdateAsync(Division entity);
    Task<Division?> FindDivisionByIdAsync(Guid? id);
    Task<IEnumerable<Division>> FindAllUserAsync();
}