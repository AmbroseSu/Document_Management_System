using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository.Impl;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly BaseDao<AttachmentDocument> _AttachmentDocumentDao;

    public AttachmentRepository(DocumentManagementSystemDbContext context)
    {
        _AttachmentDocumentDao = new BaseDao<AttachmentDocument>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(AttachmentDocument entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _AttachmentDocumentDao.AddAsync(entity);
    }

    public async Task UpdateAsync(AttachmentDocument entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _AttachmentDocumentDao.UpdateAsync(entity);
    }

    public async Task<AttachmentDocument?> FindAttachmentDocumentByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _AttachmentDocumentDao.FindByAsync(u => u.AttachmentDocumentId == id);
    }
    
    public async Task<AttachmentDocument?> FindAttachmentDocumentByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _AttachmentDocumentDao.FindByAsync(u => u.AttachmentDocumentName!.ToLower().Equals(name.ToLower()));
    }

    public async Task<IEnumerable<AttachmentDocument>> FindAllAttachmentDocumentAsync()
    {
        return await _AttachmentDocumentDao.FindAsync(u => true);
    }
    
    public async Task<IEnumerable<AttachmentDocument>> FindAttachmentDocumentsByIdsAsync(List<Guid> AttachmentDocumentIds)
    {
        if (AttachmentDocumentIds == null || !AttachmentDocumentIds.Any())
            return new List<AttachmentDocument>();

        return await _AttachmentDocumentDao.FindAsync(
            d => AttachmentDocumentIds.Contains(d.AttachmentDocumentId)
        );
    }

    public async Task<IEnumerable<AttachmentDocument>> GetAttachmentDocumentByDocumentId(Guid documentId)
    {
        return await _AttachmentDocumentDao.FindAsync(
            d => true
        );
    }
}