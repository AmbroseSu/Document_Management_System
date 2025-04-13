using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class StepRepository : IStepRepository
{
    private readonly BaseDao<Step> _stepDao;

    public StepRepository(DocumentManagementSystemDbContext context)
    {
        _stepDao = new BaseDao<Step>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(Step entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _stepDao.AddAsync(entity);
    }
    
    public async Task AddRangeAsync(List<Step> steps)
    {
        if (steps == null) throw new ArgumentNullException(nameof(steps));
        await _stepDao.AddRangeAsync(steps);
    }

    public async Task UpdateAsync(Step entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _stepDao.UpdateAsync(entity);
    }
    
    public async Task UpdateRangeAsync(List<Step> steps)
    {
        if (steps == null) throw new ArgumentNullException(nameof(steps));
        await _stepDao.UpdateRangeAsync(steps);
    }
    
    public async Task<IEnumerable<Step>> FindAllStepAsync()
    {
        return await _stepDao.FindAsync(u => true,
            u => u.Include(d => d.Role));
    }
    public async Task<Step?> FindStepByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _stepDao.FindByAsync(s => s.StepId == id, st => st.Include(st => st.Tasks));
    }
    
    public async Task<IEnumerable<Step>> FindByFlowIdsAsync(List<Guid> flowIds)
    {
        return await _stepDao.FindAsync(s => flowIds.Contains(s.FlowId), st => st.Include(st => st.Tasks).Include(r => r.Role));
        /*return await _context.Steps
            .Where(s => flowIds.Contains(s.FlowId))
            .ToListAsync();*/
    }
    
    public async Task<IEnumerable<Step>?> FindStepByFlowIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _stepDao.FindAsync(s => s.FlowId == id, st => st.Include(st => st.Tasks));
    }
    
    public async Task<IEnumerable<Step>> FindAllStepsInFlowAsync(Guid flowId)
    {
        if (flowId == Guid.Empty) throw new ArgumentNullException(nameof(flowId));
        
        return await _stepDao.FindAsync(
            t => t.FlowId  == flowId,
            q => q.OrderBy(s => s.StepNumber).Include(t => t.Flow)
                .ThenInclude(f => f.WorkflowFlows)
                .ThenInclude(wff => wff.Workflow)
        );
    }
    
    public async Task<Step?> GetFirstStepOfFlowAsync(Guid nextFlowId)
    {
        return await _stepDao.FindByAsync(
            s => s.FlowId == nextFlowId,
            q => q.OrderBy(s => s.StepNumber)
                .Include(s => s.Flow)
                .ThenInclude(f => f.WorkflowFlows)
                .ThenInclude(wff => wff.Workflow)
        );
    }
    
    
    
}