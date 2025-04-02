using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class DocumentTypeRepository : IDocumentTypeRepository
{
    private readonly BaseDao<DocumentType> _documentTypeDao;

    public DocumentTypeRepository(DocumentManagementSystemDbContext context)
    {
        _documentTypeDao = new BaseDao<DocumentType>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(DocumentType entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentTypeDao.AddAsync(entity);
    }

    public async Task UpdateAsync(DocumentType entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _documentTypeDao.UpdateAsync(entity);
    }

    public async Task<DocumentType?> FindDocumentTypeByIdAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _documentTypeDao.FindByAsync(u => u.DocumentTypeId == id);
    }
    
    public async Task<DocumentType?> FindDocumentTypeByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _documentTypeDao.FindByAsync(u => u.DocumentTypeName!.ToLower().Equals(name.ToLower()));
    }

    public async Task<IEnumerable<DocumentType>> FindAllDocumentTypeAsync()
    {
        return await _documentTypeDao.FindAsync(u => true,
            u => u.Include(d => d.ArchivedDocuments).Include(d => d.DocumentTypeWorkflows));
    }
}