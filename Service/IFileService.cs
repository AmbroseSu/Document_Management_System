using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IFileService
{
    Task<string> SaveUploadFile(IFormFile file);
    Task<string> SaveAvatar(IFormFile file,string id);
    Task<string> GetFile(Guid id);
    Task<IActionResult> GetAvatar(string fileName);

}