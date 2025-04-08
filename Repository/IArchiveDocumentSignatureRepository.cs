using BusinessObject;

namespace Repository;

public interface IArchiveDocumentSignatureRepository
{
    Task AddAsync(ArchiveDocumentSignature entity);
}