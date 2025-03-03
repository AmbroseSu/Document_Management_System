using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IRoleResourceService
{
    Task ScanAndSaveRoleResourcesAsync();
    Task<ResponseDto> UpdateRoleResourceAsync(List<RoleResourceRequest> roleResourceRequests);
}