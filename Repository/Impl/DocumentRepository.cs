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
        return await _documentDao.FindByAsync(u => u.DocumentId  == id,
            q => q
                .Include(d => d.DocumentVersions).ThenInclude(v => v.DocumentSignatures).ThenInclude(s => s.DigitalCertificate).ThenInclude(s => s.User).ThenInclude(u => u.Division)
                .Include(d => d.Tasks).ThenInclude(t => t.User).ThenInclude(u => u.Division)
                .Include(d => d.DocumentWorkflowStatuses).ThenInclude(y => y.Workflow)
                .Include(d => d.Tasks)
                .ThenInclude(s => s.Step)
                .ThenInclude(f => f.Flow)
                .Include(d => d.User).ThenInclude(u => u.Division)
                .Include(d => d.DocumentType)
                .Include(d => d.TemplateArchiveDocument)
                .Include(q => q.FinalArchiveDocument));
    }
    
    public async Task<Document?> FindDocumentByNameAsync(string? name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return await _documentDao.FindByAsync(u => u.DocumentName!.ToLower().Equals(name.ToLower()));
    }
    
    public async Task<IEnumerable<Document>> FindDocumentByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        return await _documentDao.FindAsync(u => u.Tasks.Any(t => t.UserId == userId)||u.UserId==userId,
            q => q
                    
                .Include(d => d.DocumentWorkflowStatuses)
                .ThenInclude(dws => dws.Workflow)
                .ThenInclude(w => w.DocumentTypeWorkflows)
                .ThenInclude(w => w.DocumentType)
                .Include(d => d.DocumentVersions)
                .ThenInclude(v => v.DocumentSignatures)
                .ThenInclude(s => s.DigitalCertificate)
                .ThenInclude(c => c.User)
                .Include(d => d.Tasks).ThenInclude(t => t.User).ThenInclude(u => u.Division)
                
        );
    }

    public async Task<IEnumerable<Document>> FindAllDocumentMySelf(Guid userId)
    {
        return await _documentDao.FindAsync(d => d.UserId == userId,
            q => q
                .Include(v => v.DocumentVersions).ThenInclude(s => s.DocumentSignatures)
                .ThenInclude(c => c.DigitalCertificate).ThenInclude(u => u.User)
                .Include(c => c.DocumentType)
                .Include(d => d.DocumentWorkflowStatuses)
                .ThenInclude(ws => ws.Workflow));

    }

    public async Task<IEnumerable<Document>> FindAllDocumentAsync()
    {
        return await _documentDao.FindAsync(u => true,
            q => q
                .Include(d => d.DocumentWorkflowStatuses)
                .ThenInclude(dws => dws.Workflow)
                .Include(d => d.Tasks)
                .ThenInclude(t => t.User)

                
            );
    }
    
    public async Task<IEnumerable<Document>> FindAllDocumentForTaskAsync(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        return await _documentDao.FindAsync(
            d => !d.IsDeleted && (d.UserId == userId || d.Tasks.Any(t => t.UserId == userId)),
            q => q.Include(u => u.User).Include(dt => dt.DocumentType).Include(d => d.DocumentWorkflowStatuses)
                .ThenInclude(dws => dws.Workflow).Include(d => d.Tasks).ThenInclude(s => s.Step).ThenInclude(f => f.Flow).Include(d => d.DocumentVersions)
        );
    }
    
}