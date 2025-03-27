/*using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Serilog;

namespace DocumentManagementSystemApplication.Middleware;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ApiLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        var user = GetUserEmail(context);
        var role = GetUserRole(context);
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = request.Headers["User-Agent"].ToString();
        var action = $"{request.Method} {request.Path}";
        var requestBodyContent = await ReadRequestBody(request);

        // Ghi log request
        var requestLog = new
        {
            Level = "Information",
            Message = $"API Request: {action} | User: {user} | Role: {role} | IP: {clientIp} | User-Agent: {userAgent} | Body: {requestBodyContent}",
            Timestamp = DateTime.UtcNow.ToString("o")
        };

        Log.Information(JsonSerializer.Serialize(requestLog));

        // Đọc response body
        var originalResponseBody = context.Response.Body;
        using var newResponseBody = new MemoryStream();
        context.Response.Body = newResponseBody;

        await _next(context);

        var responseBodyContent = await ReadResponseBody(context);
        var statusCode = context.Response.StatusCode;

        // Ghi log response
        var responseLog = new
        {
            Level = "Information",
            Message = $"API Response: {action} | User: {user} | Role: {role} | IP: {clientIp} | StatusCode: {statusCode} | Body: {responseBodyContent}",
            Timestamp = DateTime.UtcNow.ToString("o")
        };

        Log.Information(JsonSerializer.Serialize(responseLog));

        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);
    }

    private string GetUserEmail(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
    }

    private string GetUserRole(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.Body == null || !request.Body.CanRead)
            return "{}";

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return string.IsNullOrEmpty(body) ? "{}" : body;
    }

    private async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return string.IsNullOrEmpty(body) ? "{}" : body;
    }
}*/

