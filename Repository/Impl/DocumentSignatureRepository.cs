using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace Repository.Impl;

public class DocumentSignatureRepository : IDocumentSignatureRepository
{
    private readonly BaseDao<DocumentSignature> _documentSignatureDao;

    public DocumentSignatureRepository(DocumentManagementSystemDbContext context)
    {
        _documentSignatureDao = new BaseDao<DocumentSignature>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(DocumentSignature entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentSignatureDao.AddAsync(entity);
    }

    public async Task UpdateAsync(DocumentSignature entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentSignatureDao.UpdateAsync(entity);
    }

    public async Task<DocumentSignature?> FindDocumentSignatureByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _documentSignatureDao.FindByAsync(u => u.DocumentSignatureId == id);
    }


    public async Task<IEnumerable<DocumentSignature>> FindAllDocumentSignatureAsync()
    {
        return await _documentSignatureDao.FindAsync(u => true);
    }
}