using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IFlowRepository
{
    Task AddAsync(Flow entity);
    Task AddRangeAsync(List<Flow> flows);
    Task UpdateAsync(Flow entity);
    Task UpdateRangeAsync(List<Flow> flows);
    Task<Flow?> FindFlowByIdAsync(Guid? id);
    Task<IEnumerable<Flow>> FindByIdsAsync(List<Guid> flowIds);
    Task<IEnumerable<Flow>> FindAllFlowAsync();
}