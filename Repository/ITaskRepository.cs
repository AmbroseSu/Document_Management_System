using BusinessObject;

namespace Repository;

public interface ITaskRepository
{
    Task AddAsync(Tasks entity);
    Task UpdateAsync(Tasks entity);
    Task<IEnumerable<Tasks>> FindTaskByStepIdAsync(Guid? id);
    Task<Tasks?> FindTaskByIdAsync(Guid? id);
}