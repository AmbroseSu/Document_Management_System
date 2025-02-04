/*using System.Linq.Expressions;
using DataAccess;
using DataAccess.DAO;

namespace Repository.Impl;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{ 
    private readonly BaseDao<T> _baseDao;

    public BaseRepository(BaseDao<T> baseDao)
    {
        _baseDao = baseDao;
    }

    protected BaseRepository()
    {
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
}*/