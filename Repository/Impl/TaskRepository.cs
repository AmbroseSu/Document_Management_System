using BusinessObject;
using BusinessObject.Enums;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class TaskRepository : ITaskRepository
{
    private readonly BaseDao<Tasks> _taskDao;

    public TaskRepository(DocumentManagementSystemDbContext context)
    {
        _taskDao = new BaseDao<Tasks>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(Tasks entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _taskDao.AddAsync(entity);
    }

    public async Task UpdateAsync(Tasks entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _taskDao.UpdateAsync(entity);
    }
    
    public async Task<IEnumerable<Tasks>> FindTaskByStepIdDocIdAsync(Guid? stepId, Guid? documentId)
    {
        if (stepId == null) throw new ArgumentNullException(nameof(stepId));
        if (documentId == null) throw new ArgumentNullException(nameof(documentId));
        return await _taskDao.FindAsync(u => u.StepId == stepId && u.DocumentId == documentId && u.IsDeleted == false,
            u => u.Include(s => s.Step).Include(d => d.Document));
    }
    
    public async Task<IEnumerable<Tasks>> FindTaskByStepIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _taskDao.FindAsync(u => u.StepId == id  && u.IsDeleted == false,
            u => u.Include(s => s.Step).Include(u => u.User).Include(d => d.Document));
    }
    
    public async Task<IEnumerable<Tasks>> FindTasksByStepIdsAsync(List<Guid> stepIds, Guid? documentId)
    {
        if (stepIds == null || stepIds.Count == 0) throw new ArgumentNullException(nameof(stepIds));
        if (documentId == null) throw new ArgumentNullException(nameof(documentId));
        
        return await _taskDao.FindAsync(s => stepIds.Contains(s.StepId) && s.DocumentId == documentId && s.IsDeleted == false, 
            st => st.Include(u => u.User).Include(st => st.Document));
    }

    
    
    
    public async Task<Tasks?> FindTaskByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _taskDao.FindByAsync(u => u.TaskId == id, 
            t => t
                .Include(u => u.User)
                .Include(d => d.Document).ThenInclude(dt => dt.DocumentType)
                .Include(d => d.Document).ThenInclude(d => d.DocumentVersions)
                .Include(d => d.Document)
                .ThenInclude(dws => dws.DocumentWorkflowStatuses)
            .ThenInclude(dwws => dwws.Workflow)
            .Include(ta => ta.Step)
            .ThenInclude(f => f.Flow)
            .ThenInclude(wff => wff.WorkflowFlows)
            .ThenInclude(w => w.Workflow));
    }
    
    public async Task<IEnumerable<Tasks>> FindAllPendingTaskByDocumentIdAsync(Guid documentId)
    {
        if (documentId == Guid.Empty) throw new ArgumentNullException(nameof(documentId));
        
        return await _taskDao.FindAsync(
            t => t.DocumentId == documentId && t.TaskStatus == TasksStatus.InProgress,
            q => q.Include(t => t.Step)
                .ThenInclude(s => s.Flow)
                .ThenInclude(f => f.WorkflowFlows)
                .ThenInclude(wff => wff.Workflow)
        );
        
    }
    public async Task<IEnumerable<Tasks>> FindNextTasksInStepAsync(Guid documentId, Guid stepId)
    {
        if (documentId == Guid.Empty) throw new ArgumentNullException(nameof(documentId));
        if (stepId == Guid.Empty) throw new ArgumentNullException(nameof(stepId));

        return await _taskDao.FindAsync(
            t => t.DocumentId == documentId && t.StepId == stepId,
            q => q.Include(t => t.Step)
                .ThenInclude(s => s.Flow)
                .ThenInclude(f => f.WorkflowFlows)
                .ThenInclude(wff => wff.Workflow)
        );
    }
    
    public async Task<IEnumerable<Tasks>> GetTasksByStepAndDocumentAsync(Guid nextStepId, Guid documentId)
    {
        return await _taskDao.FindAsync(
            t => t.StepId == nextStepId && t.DocumentId == documentId && t.IsDeleted == false,
            q => q.Include(t => t.Step)
                .ThenInclude(s => s.Flow)
                .ThenInclude(f => f.WorkflowFlows)
                .ThenInclude(wff => wff.Workflow)
        );
    }
    
    public async Task<IEnumerable<Tasks>> FindAllTaskAsync(Guid? userId)
    {
       return await _taskDao.FindAsync(
            t => t.UserId == userId && t.IsDeleted == false,
            q => q.Include(d => d.Document).ThenInclude(dt => dt.DocumentType)
                .Include(d => d.Document)
                .ThenInclude(dws => dws.DocumentWorkflowStatuses)
                .ThenInclude(dwws => dwws.Workflow)
                .Include(ta => ta.Step)
                .ThenInclude(f => f.Flow)
                .ThenInclude(wff => wff.WorkflowFlows)
                .ThenInclude(w => w.Workflow)
            );
       
    }

    public async Task<IEnumerable<Tasks>> FindTaskByDocumentIdAndUserIdAsync(Guid? documentId, Guid userId)
    {
        return await _taskDao.FindAsync(t => t.DocumentId == documentId && t.UserId == userId && t.IsDeleted == false);
    }
    

}