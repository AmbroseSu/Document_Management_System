using Microsoft.AspNetCore.Http;

namespace Service;

public interface IFileService
{
    Task<string> SaveFile(IFormFile file);
    Task<string> GetFile(Guid id);
}