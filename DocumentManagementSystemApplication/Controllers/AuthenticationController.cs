using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("view-sign-in")]
    public async Task<ResponseDto> SiginAsync([FromBody] SignInRequest signInRequest)
    {
        return await _authenticationService.SignIn(signInRequest);
        //return result;
    }

    [HttpPost("create-send-otp")]
    public async Task<ResponseDto> SendOtpAsync([FromQuery] string email)
    {
        return await _authenticationService.SendOtpAsync(email);
    }

    [HttpPost("create-verify-otp")]
    public async Task<ResponseDto> VerifyOtpAsync([FromBody] VerifyOtpRequest verifyOtpRequest)
    {
        return await _authenticationService.VerifyOtpAsync(verifyOtpRequest);
    }

    [HttpPost("create-change-password")]
    public async Task<ResponseDto> ChangePasswordAsync([FromBody] ChangePasswordRequest changePasswordRequest)
    {
        return await _authenticationService.ChangePasswordAsync(changePasswordRequest);
    }

    [HttpPost("create-forgot-password")]
    public async Task<ResponseDto> ForgotPasswordAsync([FromBody] ForgotPasswordRequest forgotPasswordRequest)
    {
        return await _authenticationService.ForgotPasswordAsync(forgotPasswordRequest);
    }
}