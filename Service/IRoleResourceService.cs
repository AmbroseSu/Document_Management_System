using DataAccess.DTO;
using DataAccess.DTO.Request;
using Service.Response;

namespace Service;

public interface IRoleResourceService
{
    Task ScanAndSaveRoleResourcesAsync();
    Task<ResponseDto> UpdateRoleResourceAsync(List<RoleResourceRequest> roleResourceRequests);
}