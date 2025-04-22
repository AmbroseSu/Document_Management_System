using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
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
    
    [HttpPost("create-import-users-from-excel")]
    public async Task<ResponseDto> CreateImportUsersFromExcel([FromBody] List<FileImportData> fileImportDatas, [FromQuery] Guid divisionId)
    {
        return await _userService.ImportUsersFromFileAsync(fileImportDatas, divisionId);
    }
    
    [HttpPost("view-users-from-excel")]
    public async Task<List<FileImportData>> ViewUsersFromExcel([FromForm] IFormFile file)
    {
        return await _userService.ReadUsersFromExcelAsync(file);
    }
    
    [HttpPost("view-users-from-csv")]
    public async Task<List<FileImportData>> ViewUsersFromCsv([FromForm] IFormFile file)
    {
        return await _userService.ReadUsersFromCsvAsync(file);
    }
    
    [HttpPost("update-avatar/{userId}")]
    public async Task<ResponseDto> UpdateAvatar([FromForm] IFormFile file,string userId)
    {
        return await _userService.UpdateAvatarAsync(file,userId);
    }
    
    [HttpGet("view-avatar/{fileName}")]
    public async Task<IActionResult> GetAvatar(string fileName)
    {
        

        return await _userService.GetAvatar(fileName);
    }
    
    [HttpPost("update-signature-img/{userId:guid}")]
    public async Task<ResponseDto> UploadSignatureImg([FromForm] IFormFile file,[FromRoute] Guid userId)
    {
        return await _userService.UploadSignatureImgAsync(file, userId);
    }
    
    [HttpGet("view-signature-img/{userId}")]
    public async Task<IActionResult> GetSignatureImg([FromRoute] string userId)
    {
        return await _userService.GetSignatureImg(userId);
    }



    
}