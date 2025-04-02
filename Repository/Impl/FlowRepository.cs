using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class FlowRepository : IFlowRepository   
{
    private readonly BaseDao<Flow> _flowDao;

    public FlowRepository(DocumentManagementSystemDbContext context)
    {
        _flowDao = new BaseDao<Flow>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(Flow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _flowDao.AddAsync(entity);
    }
    
    public async Task AddRangeAsync(List<Flow> flows)
    {
        if (flows == null) throw new ArgumentNullException(nameof(flows));
        await _flowDao.AddRangeAsync(flows);
    }

    public async Task UpdateAsync(Flow entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _flowDao.UpdateAsync(entity);
    }
    
    public async Task UpdateRangeAsync(List<Flow> flows)
    {
        if (flows == null) throw new ArgumentNullException(nameof(flows));
        await _flowDao.UpdateRangeAsync(flows);
    }
    
    public async Task<Flow?> FindFlowByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _flowDao.FindByAsync(f => f.FlowId == id, fl => fl.Include(wf => wf.Steps).Include(wf => wf.WorkflowFlows));
    }
    
    public async Task<IEnumerable<Flow>> FindByIdsAsync(List<Guid> flowIds)
    {
        return await _flowDao.FindAsync(f => flowIds.Contains(f.FlowId), fl => fl.Include(wf => wf.Steps));
        // return await _context.Flows
        //     .Where(f => flowIds.Contains(f.FlowId))
        //     .ToListAsync();
    }
}