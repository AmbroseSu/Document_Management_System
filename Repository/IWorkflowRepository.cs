using BusinessObject;
using BusinessObject.Enums;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IWorkflowRepository
{
    Task AddAsync(Workflow entity);
    Task UpdateAsync(Workflow entity);
    Task<Workflow?> FindWorkflowByIdAsync(Guid? id);
    Task<Workflow?> FindWorkflowByNameAsync(string? name);
    Task<Workflow?> FindWorkflowByScopeAsync(Scope? scope);
    Task<IEnumerable<Workflow>> FindAllWorkflowAsync();
}