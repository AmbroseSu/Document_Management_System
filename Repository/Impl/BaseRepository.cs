using System.Linq.Expressions;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class BaseRepository<T> : IBaserepository<T> where T : class
{ 
    private readonly BaseDao<T> _baseDao;

    public BaseRepository(DocumentManagementSystemDbContext context)
    {
        _baseDao = new BaseDao<T>(context);
    }
    
    public async Task AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _baseDao.AddAsync(entity);
    }
    
    public async Task UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _baseDao.UpdateAsync(entity);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));
        return await _baseDao.FindAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _baseDao.GetAllAsync();
    }
}