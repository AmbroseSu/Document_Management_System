using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Service;

public interface IFileService
{
    IActionResult InsertTextToImage(IFormFile file, string text);
    Task<string> SaveUploadFile(IFormFile file);
    Task<string> SaveAvatar(IFormFile file,string id);
    // Task<IActionResult> GetPdfFile(Guid id);
    Task<IActionResult> GetPdfFile(string path);
    Task<(byte[] FileBytes, string FileName, string ContentType)> GetFileBytes(string filePath);
    Task<IActionResult> GetAvatar(string fileName);
    string CreateAVersionFromUpload(string fileName, Guid versionId, Guid documentId,string versionName);
    string ArchiveDocument(string fileName, Guid documentId, Guid versionId, Guid archiveId);

    string? GetFileSize(Guid documentId, Guid versionId, string fileName);
    Task<string> SaveSignature(IFormFile file, string userId);
    Task<IActionResult> GetSignature(string userId);
    Task<IActionResult> ConvertDocToPdf(IFormFile file);
    Task<IActionResult> ConvertDocToPdf(string path);

    void ConvertDocToDocx(string inputPath, string outputDir);

    void InsertTextAsImageToPdf(string pdfPath,string outPath, string text, float llx, float lly, float urx, float ury);
}