using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermissionController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }
        
        [HttpPost("create-role-with-permission")]
        public async Task<ResponseDto> CreateRoleWithPermission([FromBody]RolePermissionDto rolePermissionDto)
        {
            return await _rolePermissionService.CreateRoleWithPermission(rolePermissionDto);
        }
        
    }
}
