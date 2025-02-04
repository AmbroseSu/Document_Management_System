/*using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystemApplication.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DocumentManagementSystemDbContext dbContext)
    {
        dbContext = new DocumentManagementSystemDbContext();
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = Guid.Parse(user.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());
            var resourcePath = context.Request.Path.Value;
            var method = context.Request.Method;

            var hasPermission = await dbContext.ResourcePermissions
                .Include(rp => rp.RolePermission)
                .ThenInclude(rp => rp.Permission)
                .Include(rp => rp.Resource)
                .AnyAsync(rp => 
                    rp.Resource.ResourceApi == resourcePath && 
                    rp.RolePermission.Permission.PermissionName == method &&
                    rp.RolePermission.Role.UserRoles.Any(ur => ur.UserId == userId));

            if (!hasPermission)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("You do not have permission to access this resource.");
                return;
            }
        }

        await _next(context);
    }
}*/