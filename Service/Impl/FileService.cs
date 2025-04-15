using BusinessObject.Option;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Service.Impl;

public class FileService : IFileService
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage");
    private readonly string _host;

    public FileService(IOptions<AppsetingOptions> options)
    {
        _host = options.Value.Host;
    }


    public async Task<string> SaveUploadFile(IFormFile file)
    {
        

        
        
        var filePath = Path.Combine(_storagePath, "document", "UploadedFiles", file.FileName);
        Directory.CreateDirectory(_storagePath);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;

    }

    public async Task<string> SaveAvatar(IFormFile file,string id)
    {
        var avatarFolder = Path.Combine(_storagePath, "avatar");
        Directory.CreateDirectory(avatarFolder);

        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{id}{fileExt}"; // tạo tên mới
        var filePath = Path.Combine(avatarFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
        
    }
    
    public string CreateAVersionFromUpload(string fileName, Guid versionId, Guid documentId,string versionName)
    {
        var path = Path.Combine(_storagePath, "document", "UploadedFiles", fileName);
        
        var targetDirectory = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString());

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // Tạo đường dẫn đích
        var targetPath = Path.Combine(targetDirectory, versionName+".pdf");

        // Di chuyển file
        File.Move(path, targetPath);
        var url = _host+"/api/Document/view-file/"+documentId+"?version=1&isArchive=false";
        // Trả về đường dẫn mới của file (hoặc bạn có thể trả về tên file tùy mục đích)
        return url;

    }
    
    public string ArchiveDocument(string fileName, Guid documentId, Guid versionId,Guid archiveId)
    {
        fileName += ".pdf";
        var path = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), fileName);
        
        var targetDirectory = Path.Combine(_storagePath, "archive_document", archiveId.ToString());

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var targetPath = Path.Combine(targetDirectory, fileName);

        File.Copy(path, targetPath);
        var url = _host+"/api/Document/view-file/"+archiveId+"?version=1&isArchive=true";
        return url;

    }

    // public async Task<IActionResult> GetPdfFile(Guid id)
    // {
    //     var filePath = Path.Combine(_storagePath, "document", "UploadedFiles", id+".pdf");;
    //
    //     if (!File.Exists(filePath))
    //     {
    //         throw new FileNotFoundException("File not found", filePath);
    //     }
    //
    //     const string contentType = "application/pdf";
    //     var bytes = await File.ReadAllBytesAsync(filePath);
    //
    //     return new FileContentResult(bytes, contentType);
    // }
    
    public async Task<IActionResult> GetPdfFile(string filePath)
    {
        var path = Path.Combine(_storagePath, filePath);;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found", path);
        }

        const string contentType = "application/pdf";
        var bytes = await File.ReadAllBytesAsync(path);

        return new FileContentResult(bytes, contentType);
    }
    
    public async Task<IActionResult> GetAvatar(string fileName)
    {
        var filePath = Path.Combine(_storagePath,"avatar", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var contentType = GetContentType(filePath);
        var bytes = await File.ReadAllBytesAsync(filePath);

        return new FileContentResult(bytes, contentType);
    }
    
    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}