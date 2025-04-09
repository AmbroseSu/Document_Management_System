using BusinessObject;

namespace Repository;

public interface IDocumentSignatureRepository
{
    Task AddAsync(DocumentSignature entity);
    Task UpdateAsync(DocumentSignature entity);
    Task<DocumentSignature?> FindDocumentSignatureByIdAsync(Guid? id);
    Task<IEnumerable<DocumentSignature>> FindAllDocumentSignatureAsync();
}