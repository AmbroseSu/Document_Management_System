using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;

    public UserController(IUserService userService, IFileService fileService)
    {
        _userService = userService;
        _fileService = fileService;
    }

    [HttpPost("create-user-by-form")]
    //[AuthorizeResource("[User] Create User By Form")]
    public async Task<ResponseDto> CreateUserByForm([FromBody] UserRequest userRequest)
    {
        return await _userService.CreateUserByForm(userRequest);
    }

    [HttpGet("view-profile-user")]
    //[AuthorizeResource("[User] View Profile User")]
    public async Task<ResponseDto> ViewProfileUser([FromQuery] Guid userId)
    {
        return await _userService.GetProfileAsync(userId);
    }

    [HttpPost("view-all-user")]
    //[AuthorizeResource("[User] View All User")]
    public async Task<ResponseDto> ViewAllUser([FromBody] UserFilterRequest userFilterRequest)
    {
        return await _userService.GetAllUserAsync(userFilterRequest);
    }

    [HttpPost("delete-user")]
    //[AuthorizeResource("[User] Delete User")]
    public async Task<ResponseDto> DeleteUser([FromQuery] Guid userId)
    {
        return await _userService.UpdateUserActiveOrDeleteAsync(userId);
    }

    [HttpPost("update-user")]
    //[AuthorizeResource("[User] Update User")]
    public async Task<ResponseDto> UpdateUser([FromBody] UserUpdateRequest userUpdateRequest)
    {
        return await _userService.UpdateUserAsync(userUpdateRequest);
    }

    [HttpPost("update-user-by-admin")]
    //[AuthorizeResource("[User] Update User By Admin")]
    public async Task<ResponseDto> UpdateUserByAdmin([FromBody] AdminUpdateUserRequest adminUpdateUserRequest)
    {
        return await _userService.AdminUpdateUserAsync(adminUpdateUserRequest);
    }
    
    [HttpPost("create-import-users-from-excel")]
    //[AuthorizeResource("[User] Create Import Users From Excel")]
    public async Task<ResponseDto> CreateImportUsersFromExcel([FromBody] List<FileImportData> fileImportDatas, [FromQuery] Guid divisionId)
    {
        return await _userService.ImportUsersFromFileAsync(fileImportDatas, divisionId);
    }
    
    [HttpPost("view-users-from-excel")]
    //[AuthorizeResource("[User] View Users From Excel")]
    public async Task<List<FileImportData>> ViewUsersFromExcel([FromForm] IFormFile file)
    {
        return await _userService.ReadUsersFromExcelAsync(file);
    }
    
    [HttpPost("view-users-from-csv")]
    //[AuthorizeResource("[User] View Users From Csv")]
    public async Task<List<FileImportData>> ViewUsersFromCsv([FromForm] IFormFile file)
    {
        return await _userService.ReadUsersFromCsvAsync(file);
    }

    [HttpPost("update-avatar/{userId}")]
    //[AuthorizeResource("[User] Update Avatar")]
    public async Task<ResponseDto> UpdateAvatar([FromForm] IFormFile file,string userId)
    {
        return await _userService.UpdateAvatarAsync(file,userId);
    }
    
    [AllowAnonymous]
    //[HttpGet("view-avatar/{fileName}")]
    //[AuthorizeResource("[User] View Avatar")]
    public async Task<IActionResult> GetAvatar(string fileName)
    {
        

        return await _userService.GetAvatar(fileName);
    }
    
    [AllowAnonymous]
    //[HttpPost("update-insert-name-signature-img")]
    //[AuthorizeResource("[User] Update Insert Name Signature Img")]
    public  IActionResult InsertNameSignatureImg(IFormFile file,[FromForm] string fullName)
    {
        try
        {
            return _fileService.InsertTextToImage(file, fullName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("update-signature-img")]
    //[AuthorizeResource("[User] Update Signature Img")]
    public async Task<ResponseDto> UploadSignatureImg([FromForm] UpdateSignatureRequest updateSignatureRequest)
    {
        var userId = User.FindFirst("userid")?.Value;

        return await _userService.UploadSignatureImgAsync(updateSignatureRequest, Guid.Parse(userId));
    }
    
    [HttpPost("update-enable-signature-img")]
    //[AuthorizeResource("[User] Update Enable Signature Img")]
    public async Task<ResponseDto> UpDateEnableSignatureImg([FromQuery] Guid userId)
    {

        return await _userService.EnableUploadSignatureImage(userId);
    }
    
    [AllowAnonymous]
    [HttpGet("view-signature-img/{fileName}")]
    //[AuthorizeResource("[User] View Signature Img")]
    public async Task<IActionResult> GetSignatureImg([FromRoute] string fileName)
    {
        return await _userService.GetSignatureImg(fileName);
    }

    [HttpGet("view-all-user-has-not-permission-archive-doc")]
    //[AuthorizeResource("[User] View All User Has Not Permission Archive Doc")]
    public async Task<ResponseDto> FindUserCanGrandPermission([FromQuery] Guid archivedDocId)
    {
        return await _userService.FindUserCanGrandPermission(archivedDocId);
    }


    
}