using BusinessObject;
using DataAccess;
using DataAccess.DAO;

namespace Repository.Impl;

public class ArchivedDocumentRepository : IArchivedDocumentRepository
{
    private readonly BaseDao<ArchivedDocument> _archivedDocumentDao;

    public ArchivedDocumentRepository(DocumentManagementSystemDbContext context)
    {
        _archivedDocumentDao = new BaseDao<ArchivedDocument>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(ArchivedDocument entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _archivedDocumentDao.AddAsync(entity);
    }

    public async Task UpdateAsync(ArchivedDocument entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _archivedDocumentDao.UpdateAsync(entity);
    }

    public async Task<ArchivedDocument?> FindArchivedDocumentByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _archivedDocumentDao.FindByAsync(u => u.ArchivedDocumentId == id);
    }
    
    public async Task<ArchivedDocument?> FindArchivedDocumentByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _archivedDocumentDao.FindByAsync(u => u.ArchivedDocumentName!.ToLower().Equals(name.ToLower()));
    }

    public async Task<IEnumerable<ArchivedDocument>> FindAllArchivedDocumentAsync()
    {
        return await _archivedDocumentDao.FindAsync(u => true);
    }
    
    public async Task<IEnumerable<ArchivedDocument>> FindArchivedDocumentsByIdsAsync(List<Guid> archivedDocumentIds)
    {
        if (archivedDocumentIds == null || !archivedDocumentIds.Any())
            return new List<ArchivedDocument>();

        return await _archivedDocumentDao.FindAsync(
            d => archivedDocumentIds.Contains(d.ArchivedDocumentId)
        );
    }
}