using BusinessObject;
using DataAccess;
using DataAccess.DAO;

namespace Repository.Impl;

public class ArchiveDocumentSignatureRepository : IArchiveDocumentSignatureRepository
{
    private readonly BaseDao<ArchiveDocumentSignature> _archiveDocumentSignature;

    public ArchiveDocumentSignatureRepository(DocumentManagementSystemDbContext context)
    {
        _archiveDocumentSignature = new BaseDao<ArchiveDocumentSignature>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(ArchiveDocumentSignature entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _archiveDocumentSignature.AddAsync(entity);
    }
}