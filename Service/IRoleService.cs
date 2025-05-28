using DataAccess.DTO;

namespace Service;

public interface IRoleService
{
    Task SeedRolesAsync();
    Task<ResponseDto> CreateRole(RoleDto roleDto,Guid userId);
    Task<ResponseDto> ViewAllRoles();
}