using Microsoft.AspNetCore.Http;

namespace Service.Impl;

public class FileService : IFileService
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage", "document", "UploadedFiles");
    public async Task<string> SaveFile(IFormFile file)
    {
        

        

        var filePath = Path.Combine(_storagePath, file.FileName);
        Directory.CreateDirectory(_storagePath);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;

    }

    public Task<string> GetFile(Guid id)
    {
        throw new NotImplementedException();
    }
}