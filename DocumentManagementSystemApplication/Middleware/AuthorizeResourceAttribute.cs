using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocumentManagementSystemApplication.Middleware;

public class AuthorizeResourceAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _requiredResource;

    public AuthorizeResourceAttribute(string resource)
    {
        _requiredResource = resource;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var resources = user.Claims.FirstOrDefault(c => c.Type == "resources")?.Value.Split(",") ?? new string[] { };

        if (!resources.Contains(_requiredResource))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}