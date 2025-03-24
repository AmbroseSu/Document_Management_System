using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IUserService
{  
    Task<ResponseDto> CreateUserByForm (UserRequest userRequest);
    Task<ResponseDto> GetProfileAsync(Guid userId);
    Task<ResponseDto> GetAllUserAsync(UserFilterRequest userFilterRequest);
    Task<ResponseDto> UpdateUserActiveOrDeleteAsync(Guid userId);
}