using BusinessObject.Option;

namespace Repository;

public interface IDocumentElasticRepository
{
    Task<bool> AddAsync(DocumentElastic document);
    Task<DocumentElastic> GetByIdAsync(Guid id);
    Task<IEnumerable<DocumentElastic>> SearchAsync(string query);
    Task<bool> UpdateAsync(DocumentElastic document);
    Task<bool> DeleteAsync(Guid id);
}