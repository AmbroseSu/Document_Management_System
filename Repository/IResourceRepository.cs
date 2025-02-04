using BusinessObject;
using DataAccess.DTO;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IResourceRepository
{
    Task AddRangeAsync(List<ResourceDto> resources);
    Task AddAsync(Resource resource);
    Task<Resource?> FindResourceByApiAsync(string resourceApi);
}