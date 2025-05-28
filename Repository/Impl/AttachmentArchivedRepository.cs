using BusinessObject;
using DataAccess;
using DataAccess.DAO;

namespace Repository.Impl;

public class AttachmentArchivedRepository : IAttachmentArchivedRepository
{
    private readonly BaseDao<AttachmentArchivedDocument> _AttachmentArchivedDocumentDao;

    public AttachmentArchivedRepository(DocumentManagementSystemDbContext context)
    {
        _AttachmentArchivedDocumentDao = new BaseDao<AttachmentArchivedDocument>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(AttachmentArchivedDocument entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _AttachmentArchivedDocumentDao.AddAsync(entity);
    }

    public async Task UpdateAsync(AttachmentArchivedDocument entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _AttachmentArchivedDocumentDao.UpdateAsync(entity);
    }

    public async Task<AttachmentArchivedDocument?> FindAttachmentArchivedDocumentByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _AttachmentArchivedDocumentDao.FindByAsync(u => u.AttachmentArchivedDocumentId == id);
    }
    
    public async Task<AttachmentArchivedDocument?> FindAttachmentArchivedDocumentByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _AttachmentArchivedDocumentDao.FindByAsync(u => u.AttachmentName!.ToLower().Equals(name.ToLower()));
    }

    public async Task<IEnumerable<AttachmentArchivedDocument>> FindAllAttachmentArchivedDocumentAsync()
    {
        return await _AttachmentArchivedDocumentDao.FindAsync(u => true);
    }
    
    public async Task<IEnumerable<AttachmentArchivedDocument>> FindAttachmentArchivedDocumentsByIdsAsync(List<Guid> AttachmentArchivedDocumentIds)
    {
        if (AttachmentArchivedDocumentIds == null || !AttachmentArchivedDocumentIds.Any())
            return new List<AttachmentArchivedDocument>();

        return await _AttachmentArchivedDocumentDao.FindAsync(
            d => AttachmentArchivedDocumentIds.Contains(d.AttachmentArchivedDocumentId)
        );
    }

    public async Task<IEnumerable<AttachmentArchivedDocument>> GetAttachmentArchivedDocumentByDocumentId(Guid documentId)
    {
        return await _AttachmentArchivedDocumentDao.FindAsync(
            d => true
        );
    }
}