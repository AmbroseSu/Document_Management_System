using System.Net.Http.Headers;
using System.Text.Json;
using BusinessObject.Option;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;

namespace Service.Impl;

public class ExternalApiService : IExternalApiService
{
    
    private readonly HttpClient _httpClient;
    private readonly string _aiScanUrl;

    public ExternalApiService(HttpClient httpClient, IOptions<AppsetingOptions> options)
    {
        _httpClient = httpClient;
        _aiScanUrl = options.Value.AiApiUrl;
    }

    public async Task<DocumentAiResponse> ScanPdfAsync(string filePath)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var content = new MultipartFormDataContent();

        // Đọc nội dung file và gắn header cho file
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        // Thêm vào form-data với key là "file"
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        var response = await _httpClient.PostAsync(_aiScanUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Lỗi gọi AI Scan API: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<DocumentAiResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }


}