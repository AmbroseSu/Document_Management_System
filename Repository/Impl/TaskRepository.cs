using BusinessObject;
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
    
    public async Task<IEnumerable<Tasks>> FindTaskByStepIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _taskDao.FindAsync(u => u.StepId == id && u.IsDeleted == false,
            u => u.Include(d => d.Document));
    }
    
    public async Task<Tasks?> FindTaskByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _taskDao.FindByAsync(u => u.TaskId == id);
    }
    
}