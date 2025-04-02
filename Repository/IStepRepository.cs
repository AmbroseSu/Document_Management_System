using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IStepRepository
{
    Task AddAsync(Step entity);
    Task AddRangeAsync(List<Step> steps);
    Task UpdateAsync(Step entity);
    Task UpdateRangeAsync(List<Step> steps);
    Task<IEnumerable<Step>> FindAllStepAsync();
    Task<Step?> FindStepByIdAsync(Guid? id);
    Task<IEnumerable<Step>> FindByFlowIdsAsync(List<Guid> flowIds);
}