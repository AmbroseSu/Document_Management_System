using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class DivisionRepository : IDivisionRepository
{
    
    private readonly BaseDao<Division> _divisionDao;

    public DivisionRepository(DocumentManagementSystemDbContext context)
    {
        _divisionDao = new BaseDao<Division>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(Division entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _divisionDao.AddAsync(entity);
    }

    public async Task UpdateAsync(Division entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _divisionDao.UpdateAsync(entity);
    }

    public async Task<Division?> FindDivisionByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _divisionDao.FindByAsync(u => u.DivisionId == id);
    }
    
    public async Task<Division?> FindDivisionByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _divisionDao.FindByAsync(u => u.DivisionName!.ToLower().Equals(name.ToLower()));
    }

    public async Task<IEnumerable<Division>> FindAllDivisionAsync()
    {
        return await _divisionDao.FindAsync(u => true,
            u => u.Include(d => d.Users));
    }
    
    public async Task<IEnumerable<Division>> FindDivisionsByIdsAsync(List<Guid> divisionIds)
    {
        if (divisionIds == null || !divisionIds.Any())
            return new List<Division>();

        return await _divisionDao.FindAsync(
            d => divisionIds.Contains(d.DivisionId)
        );
    }
}