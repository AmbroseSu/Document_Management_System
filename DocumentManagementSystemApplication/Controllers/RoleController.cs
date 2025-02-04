using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        
        [HttpPost("create-role")]
        public async Task<ResponseDto> CreateRole([FromBody]RoleDto roleDto)
        {
            return await _roleService.CreateRole(roleDto);
        }
        
    }
}
