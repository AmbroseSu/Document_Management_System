using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Http;

namespace Service;

public interface IUserService
{
    Task<ResponseDto> CreateUserByForm(UserRequest userRequest);
    Task<ResponseDto> GetProfileAsync(Guid userId);
    Task<ResponseDto> GetAllUserAsync(UserFilterRequest userFilterRequest);
    Task<ResponseDto> UpdateUserActiveOrDeleteAsync(Guid userId);
    Task<ResponseDto> UpdateUserAsync(UserUpdateRequest userUpdateRequest);
    Task<ResponseDto> AdminUpdateUserAsync(AdminUpdateUserRequest adminUpdateUserRequest);
    Task<ResponseDto> ImportUsersFromExcelAsync(IFormFile file, Guid divisionId);
}