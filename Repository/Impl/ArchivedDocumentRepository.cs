using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

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
        return await _archivedDocumentDao.FindByAsync(u => u.ArchivedDocumentId == id,
            q => q
                .Include(d => d.UserDocumentPermissions).ThenInclude(q => q.User).ThenInclude(u => u.Division)
            .Include(d => d.DocumentType)
                .Include(a => a.ArchiveDocumentSignatures)
                .ThenInclude(x => x.DigitalCertificate));
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
    
    public async Task<IEnumerable<ArchivedDocument>> FindArchivedDocumentByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        return await _archivedDocumentDao.FindAsync(u => u.UserDocumentPermissions.Any(t => t.UserId == userId),
            q => q
                .Include(d => d.UserDocumentPermissions)
                .ThenInclude(up => up.User)
                .ThenInclude(u => u.Division)
                .Include(d => d.DocumentType)
                .Include(d => d.ArchiveDocumentSignatures)
                .ThenInclude(d => d.DigitalCertificate)
                .ThenInclude(d => d.User));
    }

    public async Task<IEnumerable<ArchivedDocument>> GetAllArchiveTemplates()
    {
        return await _archivedDocumentDao.FindAsync(d => d.IsTemplate,
            q => q
                .Include(a => a.DocumentType));
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