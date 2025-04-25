using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IUserService
{
    Task<ResponseDto> CreateUserByForm(UserRequest userRequest);
    Task<ResponseDto> GetProfileAsync(Guid userId);
    Task<ResponseDto> GetAllUserAsync(UserFilterRequest userFilterRequest);
    Task<ResponseDto> UpdateUserActiveOrDeleteAsync(Guid userId);
    Task<ResponseDto> UpdateUserAsync(UserUpdateRequest userUpdateRequest);
    Task<ResponseDto> AdminUpdateUserAsync(AdminUpdateUserRequest adminUpdateUserRequest);
    Task<ResponseDto> UpdateAvatarAsync(IFormFile file,string id);
    Task<ResponseDto> ImportUsersFromFileAsync(List<FileImportData> fileImportDatas, Guid divisionId);
    Task<List<FileImportData>> ReadUsersFromExcelAsync(IFormFile file);
    Task<List<FileImportData>> ReadUsersFromCsvAsync(IFormFile file);
    Task<IActionResult> GetAvatar(string userId);
    Task<ResponseDto> UploadSignatureImgAsync(IFormFile file, Guid userId,bool? isDigital);
    Task<ResponseDto> EnableUploadSignatureImage(Guid userId);
    Task<IActionResult> GetSignatureImg(string userId);
}