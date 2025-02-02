using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IUserService
{  
    Task<ResponseDto> CreateUserByForm (UserRequest userRequest);
}