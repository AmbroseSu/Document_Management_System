using DataAccess.DTO;

namespace Service;

public interface IResourceService
{
    Task ScanAndSaveResourcesAsync();
    Task<ResponseDto> CreateResource(ResourceDto resourceDto,Guid userId);
}