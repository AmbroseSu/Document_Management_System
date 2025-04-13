using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("create-user-by-form")]
    public async Task<ResponseDto> CreateUserByForm([FromBody] UserRequest userRequest)
    {
        return await _userService.CreateUserByForm(userRequest);
    }

    [HttpGet("view-profile-user")]
    public async Task<ResponseDto> ViewProfileUser([FromQuery] Guid userId)
    {
        return await _userService.GetProfileAsync(userId);
    }

    [HttpPost("view-all-user")]
    public async Task<ResponseDto> ViewAllUser([FromBody] UserFilterRequest userFilterRequest)
    {
        return await _userService.GetAllUserAsync(userFilterRequest);
    }

    [HttpPost("delete-user")]
    public async Task<ResponseDto> DeleteUser([FromQuery] Guid userId)
    {
        return await _userService.UpdateUserActiveOrDeleteAsync(userId);
    }

    [HttpPost("update-user")]
    public async Task<ResponseDto> UpdateUser([FromBody] UserUpdateRequest userUpdateRequest)
    {
        return await _userService.UpdateUserAsync(userUpdateRequest);
    }

    [HttpPost("update-user-by-admin")]
    public async Task<ResponseDto> UpdateUserByAdmin([FromBody] AdminUpdateUserRequest adminUpdateUserRequest)
    {
        return await _userService.AdminUpdateUserAsync(adminUpdateUserRequest);
    }
    
    [HttpPost("update-avatar")]
    public async Task<ResponseDto> UpdateAvatar([FromForm] IFormFile file)
    {
        return await _userService.UpdateAvatarAsync(file);
    }
}