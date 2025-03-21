using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IAuthenticationService
{
    Task<ResponseDto> SignIn(SignInRequest signInRequest);
}