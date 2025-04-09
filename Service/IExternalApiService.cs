using DataAccess.DTO.Response;
using Microsoft.AspNetCore.Http;

namespace Service;

public interface IExternalApiService
{
    Task<DocumentAiResponse> ScanPdfAsync(string filePath);
}