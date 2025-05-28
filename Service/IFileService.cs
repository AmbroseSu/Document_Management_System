using BusinessObject;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
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
    Task<string> ConvertDocToPdfPhysic(string path);
    Task<(byte[] FileBytes, string FileName, string ContentType)> GetFileBytes(string filePath);
    Task<IActionResult> GetAvatar(string fileName);
    string CreateFirstVersion(Guid documentId, string documentName, Guid versionId, Guid templateId);
    string CreateAVersionFromUpload(string fileName, Guid versionId, Guid documentId,string versionName,DateTime? validFroms);
    string ArchiveDocument(string fileName, Guid documentId, Guid versionId, Guid archiveId);

    string? GetFileSize(Guid documentId, Guid versionId, string fileName);
    Task<string> SaveSignature(IFormFile file, string userId);
    Task<IActionResult> GetSignature(string userId);
    Task<IActionResult> ConvertDocToPdf(IFormFile file);
    Task<IActionResult> ConvertDocToPdf(string path);

    void ConvertDocToDocx(string inputPath, string outputDir);

    void InsertTextAsImageToPdf(string pdfPath,string outPath, string text,int pageNum, float llx, float lly, float urx, float ury);

    Task<string> InsertNumberDocument(IFormFile file, Guid templateId, string numberDoc, int pageNum, int llx, int lly,
        int urx, int ury);

    Task<string> SaveNewVersionDocFromBase64(DocumentCompareDto docCompareDto,DocumentVersion documentVersion);
}