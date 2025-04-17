using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository.Impl;

public class DocumentVersionRepository: IDocumentVersionRepository
{
    private readonly BaseDao<DocumentVersion> _documentVersionDao;

    public DocumentVersionRepository(DocumentManagementSystemDbContext context)
    {
        _documentVersionDao = new BaseDao<DocumentVersion>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(DocumentVersion documentVersion)
    {
        if (documentVersion == null) throw new ArgumentNullException(nameof(documentVersion));
        await _documentVersionDao.AddAsync(documentVersion);
    }

    public async Task UpdateAsync(DocumentVersion documentVersion)
    {
        if (documentVersion == null) throw new ArgumentNullException(nameof(documentVersion));
        await _documentVersionDao.UpdateAsync(documentVersion);
    }
    
    public async Task<DocumentVersion?> FindDocumentVersionByIdAsync(Guid? documentVersionId)
    {
        if (documentVersionId == null) throw new ArgumentNullException(nameof(documentVersionId));
        return await _documentVersionDao.FindByAsync(d => d.DocumentVersionId == documentVersionId);
    }
    
    public async Task<IEnumerable<DocumentVersion>?> FindDocumentVersionByDocumentIdAsync(Guid? documentId)
    {
        if (documentId == null) throw new ArgumentNullException(nameof(documentId));
        return await _documentVersionDao.FindAsync(d => d.DocumentId == documentId, dm => dm.Include(dv => dv.Comments).ThenInclude(u => u.User));
    }
    
}