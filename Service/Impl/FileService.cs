using System.Diagnostics;
using System.Globalization;
using BusinessObject.Option;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;

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

    public async Task<string> SaveAvatar(IFormFile file, string id)
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

    public string CreateAVersionFromUpload(string fileName, Guid versionId, Guid documentId, string versionName)
    {
        var path = Path.Combine(_storagePath, "document", "UploadedFiles", fileName);

        var targetDirectory = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString());

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // Tạo đường dẫn đích
        var targetPath = Path.Combine(targetDirectory, versionName + ".pdf");

        // Di chuyển file
        File.Move(path, targetPath);
        var url = _host + "/api/Document/view-file/" + documentId + "?version=1&isArchive=false";
        // Trả về đường dẫn mới của file (hoặc bạn có thể trả về tên file tùy mục đích)
        return url;
    }

    public string ArchiveDocument(string fileName, Guid documentId, Guid versionId, Guid archiveId)
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
        var url = _host + "/api/Document/view-file/" + archiveId + "?version=1&isArchive=true";
        return url;
    }

    public string? GetFileSize(Guid documentId, Guid versionId, string fileName)
    {
        fileName += ".pdf";
        var path = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), fileName);
        var info = new FileInfo(path);
        if (!info.Exists) return null;
        var size = info.Length;
        var sizeInKB = Math.Round(size / 1024.0, 2); // Chuyển đổi sang KB
        var sizeInMB = Math.Round(sizeInKB / 1024.0, 2); // Chuyển đổi sang MB
        var sizeInGB = Math.Round(sizeInMB / 1024.0, 2); // Chuyển đổi sang GB
        if (sizeInMB > 1024)
            return sizeInGB.ToString(CultureInfo.InvariantCulture) + " GB";
        if (sizeInKB > 1024)
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
        var filePath = Path.Combine(_storagePath, "signature", userId);

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
        var path = Path.Combine(_storagePath, filePath);
        var extension = Path.GetExtension(path);
        var tmp = extension;
        switch (extension)
        {
            case ".pdf":
                extension = "pdf";
                break;
            case ".docx":
                extension = "openxmlformats-officedocument.wordprocessingml.document";
                break;
            case "doc":
                extension = "vnd.ms-word";
                break;
        }
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found", path);
        }

        var contentType = $"application/{extension}";
        var bytes = await File.ReadAllBytesAsync(path);

        return new FileContentResult(bytes, contentType)
            {
                FileDownloadName = Guid.NewGuid() + tmp
            }
            ;
    }
    
    public async Task<(byte[] FileBytes, string FileName, string ContentType)> GetFileBytes(string filePath)
    {
        var path = Path.Combine(_storagePath, filePath);

        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        var extension = Path.GetExtension(path).ToLower();
        string contentType = extension switch
        {
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var bytes = await File.ReadAllBytesAsync(path);
        var fileName = Path.GetFileName(path);

        return (bytes, fileName, contentType);
    }

    public async Task<IActionResult> GetAvatar(string fileName)
    {
        var filePath = Path.Combine(_storagePath, "avatar", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var contentType = GetContentType(filePath);
        var bytes = await File.ReadAllBytesAsync(filePath);

        return new FileContentResult(bytes, contentType);
    }

    public async Task<IActionResult> ConvertDocToPdf(IFormFile file)
    {
        // Ensure the file is a .doc or .docx
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (fileExtension != ".doc" && fileExtension != ".docx")
        {
            throw new ArgumentException("Invalid file format. Only .doc and .docx are supported.");
        }
    
        // Convert Word to PDF directly from the file stream
        await using var inputStream = file.OpenReadStream();
        using var wordDocument = new WordDocument(inputStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();
        var pdfDocument = renderer.ConvertToPDF(wordDocument);
        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        return new FileContentResult(pdfStream.ToArray(), "application/pdf")
            ;
    }
    
    public void ConvertDocToDocx(string inputPath, string outputDir)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "libreoffice",
                Arguments = $"--headless --convert-to docx --outdir \"{outputDir}\" \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"LibreOffice failed: {stderr}");
        }
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