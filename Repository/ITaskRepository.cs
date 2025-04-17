using BusinessObject;

namespace Repository;

public interface ITaskRepository
{
    Task AddAsync(Tasks entity);
    Task UpdateAsync(Tasks entity);
    Task<IEnumerable<Tasks>> FindTaskByStepIdAsync(Guid? id);
    
    Task<IEnumerable<Tasks>> FindTaskByStepIdDocIdAsync(Guid? stepId, Guid? documentId);
    Task<Tasks?> FindTaskByIdAsync(Guid? id);
    Task<IEnumerable<Tasks>> FindAllPendingTaskByDocumentIdAsync(Guid documentId);
    Task<IEnumerable<Tasks>> FindNextTasksInStepAsync(Guid documentId, Guid stepId);
    Task<IEnumerable<Tasks>> GetTasksByStepAndDocumentAsync(Guid nextStepId, Guid documentId);
    Task<IEnumerable<Tasks>> FindAllTaskAsync(Guid? userId);
    Task<IEnumerable<Tasks>> FindTaskByDocumentIdAndUserIdAsync(Guid? documentId,Guid userId);
}