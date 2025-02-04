using DataAccess.DTO;

namespace Service;

public interface IPermissionService
{
    Task SeedPermissionsAsync();
    //Task<ResponseDto> CreatePermission(string permission);
    Task<ResponseDto> CreatePermission(PermissionDto permissionDto);
}