using BusinessObject;
using DataAccess.DTO;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IResourceRepository
{
    Task AddRangeAsync(List<ResourceDto> resources);
    Task AddAsync(Resource resource);
    Task<IEnumerable<Resource>> GetAllAsync();
    Task<Resource?> FindResourceByApiAsync(string resourceApi);
    Task<Resource?> FindResourceByIdAsync(Guid? resourceId);
}