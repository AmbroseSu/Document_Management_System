/*using System.Text.Json;
using Serilog;

namespace DocumentManagementSystemApplication.Middleware;

public class ServerEventLogger
{
    private readonly RequestDelegate _next;

    public ServerEventLogger(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var user = context.User.Identity?.Name ?? "Anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var errorMessage = $"Exception in {context.Request.Path} | User: {user} | IP: {ipAddress} | Error: {ex.Message} | StackTrace: {ex.StackTrace}";

            Log.Error(errorMessage);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }
}*/

