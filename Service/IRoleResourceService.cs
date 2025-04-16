using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IRoleResourceService
{
    Task ScanAndSaveRoleResourcesAsync();
    Task ScanAndSaveRoleResourcesForOneRoleAsync(Role role);
    Task<ResponseDto> UpdateRoleResourceAsync(List<RoleResourceRequest> roleResourceRequests);
    Task<ResponseDto> GetRoleResourceAsync(RoleFillter roleFillter);
}