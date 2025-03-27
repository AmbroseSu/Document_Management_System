using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleResourceController : ControllerBase
{
    private readonly IRoleResourceService _roleResourceService;

    public RoleResourceController(IRoleResourceService roleResourceService)
    {
        _roleResourceService = roleResourceService;
    }

    [HttpPost("create-role-with-resources")]
    public async Task<ResponseDto> CreateRoleWithResource([FromBody] List<RoleResourceRequest> roleResourceRequests)
    {
        return await _roleResourceService.UpdateRoleResourceAsync(roleResourceRequests);
    }
}