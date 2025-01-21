using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO;

public class BaseDao<T> where T : class
{
    private readonly DocumentManagementSystemDbContext _context;
    private static BaseDao<T> _instance;
    private static object _instanceLock = new object();

    public static BaseDao<T> Instance
    {
        get
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new BaseDao<T>();
                }
            }
            return _instance;
        }
    }
    
    public BaseDao(DocumentManagementSystemDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private BaseDao() { }

    public async Task AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        return await _context.Set<T>().Where(predicate).ToListAsync();
    }
    public async Task<T?> FindByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
}