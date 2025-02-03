using DataAccess.DTO;

namespace Repository;

public interface IResourceRepository
{
    Task AddAsync(List<ResourceDto> resources);
}