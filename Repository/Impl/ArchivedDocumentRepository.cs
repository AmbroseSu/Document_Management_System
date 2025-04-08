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
}