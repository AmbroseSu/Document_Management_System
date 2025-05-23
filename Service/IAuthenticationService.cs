using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IAuthenticationService
{
    Task<ResponseDto> SignIn(SignInRequest signInRequest);
    Task<ResponseDto> SendOtpAsync(string email);
    Task<ResponseDto> VerifyOtpAsync(VerifyOtpRequest verifyOtpRequest);
    Task<ResponseDto> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest);
    Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);
}