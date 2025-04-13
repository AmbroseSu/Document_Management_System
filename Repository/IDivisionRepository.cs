using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IDivisionRepository
{
    Task AddAsync(Division entity);
    Task UpdateAsync(Division entity);
    Task<Division?> FindDivisionByIdAsync(Guid? id);
    Task<Division?> FindDivisionByNameAsync(string? name);
    Task<IEnumerable<Division>> FindAllDivisionAsync();
    Task<IEnumerable<Division>> FindDivisionsByIdsAsync(List<Guid> divisionIds);
}