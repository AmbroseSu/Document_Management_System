using BusinessObject;

namespace Repository;

public interface IArchivedDocumentRepository
{
    Task AddAsync(ArchivedDocument entity);
}