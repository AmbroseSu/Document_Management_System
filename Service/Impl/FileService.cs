using System.Globalization;
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

    public string? GetFileSize(Guid documentId, Guid versionId, string fileName)
    {
        fileName += ".pdf";
        var path = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), fileName);
        var info = new FileInfo(path);
        if (!info.Exists) return null;
        var size = info.Length;
        var sizeInKB = Math.Round(size / 1024.0,2); // Chuyển đổi sang KB
        var sizeInMB = Math.Round(sizeInKB / 1024.0,2); // Chuyển đổi sang MB
        var sizeInGB = Math.Round(sizeInMB / 1024.0,2); // Chuyển đổi sang GB
        if(sizeInMB > 1024)
            return sizeInGB.ToString(CultureInfo.InvariantCulture)+" GB";
        if(sizeInKB > 1024)
            return sizeInMB.ToString(CultureInfo.InvariantCulture) + " Mb";
        return sizeInKB.ToString(CultureInfo.InvariantCulture) + " KB";

    }

    public async Task<string> SaveSignature(IFormFile file, string userId)
    {
        var avatarFolder = Path.Combine(_storagePath, "signature");
        Directory.CreateDirectory(avatarFolder);

        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{userId}{fileExt}"; // tạo tên mới
        var filePath = Path.Combine(avatarFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public async Task<IActionResult> GetSignature(string userId)
    {
        var filePath = Path.Combine(_storagePath,"signature", userId);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var contentType = GetContentType(filePath);
        var bytes = await File.ReadAllBytesAsync(filePath);

        return new FileContentResult(bytes, contentType);
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