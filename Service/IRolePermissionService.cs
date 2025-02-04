using DataAccess.DTO;

namespace Service;

public interface IRolePermissionService
{
    Task<ResponseDto> CreateRoleWithPermission(RolePermissionDto rolePermissionDto);
}