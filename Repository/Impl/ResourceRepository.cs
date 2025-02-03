using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using DataAccess.DTO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class ResourceRepository : IResourceRepository
{
    
    private readonly BaseDao<Resource> _resourceDao;
    
    public ResourceRepository(DocumentManagementSystemDbContext context)
    {
        _resourceDao = new BaseDao<Resource>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(List<ResourceDto> resources)
    {
        if (resources == null || resources.Count == 0)
            throw new ArgumentNullException(nameof(resources), "Resource list cannot be null or empty.");

        // Lấy danh sách API đã có trong DB
        var existingResources = await _resourceDao.GetAllAsync();

        // Tìm API mới chưa có trong DB
        var newResources = resources
            .Where(api => !existingResources.Any(r => r.ResourceApi == api.ResourceApi && r.ResourceMethod == api.ResourceMethod))
            .Select(api => new Resource
            {
                ResourceId = Guid.NewGuid(),
                ResourceName = api.ResourceName,
                ResourceApi = api.ResourceApi,
                ResourceMethod = api.ResourceMethod
            })
            .ToList();

        if (newResources.Count > 0)
        {
            await _resourceDao.AddRangeAsync(newResources);
        }

        // (Optional) Xóa API cũ nếu không còn tồn tại trong danh sách mới
        var removedResources = existingResources
            .Where(r => !resources.Any(api => api.ResourceApi == r.ResourceApi && api.ResourceMethod == r.ResourceMethod))
            .ToList();

        if (removedResources.Count > 0)
        {
            await _resourceDao.RemoveRangeAsync(removedResources);
        }
    }
}