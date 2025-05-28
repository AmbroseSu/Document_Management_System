using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost("create-role")]
    //[AuthorizeResource("[Role] Create Role")]
    public async Task<ResponseDto> CreateRole([FromBody] RoleDto roleDto)
    {
        var id = User.FindFirst("userid")?.Value;
        return await _roleService.CreateRole(roleDto,Guid.Parse(id));
    }
    [HttpGet("view-all-roles")]
    //[AuthorizeResource("[Role] View All Roles")]
    public async Task<ResponseDto> ViewAllRoles()
    {
        return await _roleService.ViewAllRoles();
    }
}