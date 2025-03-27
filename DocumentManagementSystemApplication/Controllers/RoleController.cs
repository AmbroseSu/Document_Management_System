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
    [AuthorizeResource("[Role] Create Role")]
    public async Task<ResponseDto> CreateRole([FromBody] RoleDto roleDto)
    {
        return await _roleService.CreateRole(roleDto);
    }
}