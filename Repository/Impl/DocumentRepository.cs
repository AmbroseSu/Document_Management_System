using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository.Impl;

public class DocumentRepository : IDocumentRepository
{
    
    private readonly BaseDao<Document> _documentDao;

    public DocumentRepository(DocumentManagementSystemDbContext context)
    {
        _documentDao = new BaseDao<Document>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(Document entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentDao.AddAsync(entity);
    }

    public async Task UpdateAsync(Document entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentDao.UpdateAsync(entity);
    }

    public async Task<Document?> FindDocumentByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _documentDao.FindByAsync(u => u.DocumentId  == id);
    }
    
    public async Task<Document?> FindDocumentByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _documentDao.FindByAsync(u => u.DocumentName!.ToLower().Equals(name.ToLower()));
    }

    public async Task<IEnumerable<Document>> FindAllDocumentAsync()
    {
        return await _documentDao.FindAsync(u => true);
    }
    
    public async Task<IEnumerable<Document>> FindAllDocumentForTaskAsync(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        return await _documentDao.FindAsync(
            d => !d.IsDeleted && (d.UserId == userId || d.Tasks.Any(t => t.UserId == userId)),
            q => q.Include(d => d.Tasks).Include(d => d.DocumentVersions)
        );
    }
    
}