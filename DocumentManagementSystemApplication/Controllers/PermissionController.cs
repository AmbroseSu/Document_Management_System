using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost("create-permission")]
    //[AuthorizeResource("[Permission] Create Permission")]
    public async Task<ResponseDto> CreatePermission([FromBody] PermissionDto permissionDto)
    {
        var id = User.FindFirst("userid")?.Value;
        return await _permissionService.CreatePermission(permissionDto,Guid.Parse(id));
    }
}