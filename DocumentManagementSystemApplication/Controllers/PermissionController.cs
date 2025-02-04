using BusinessObject;
using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }
        
        [HttpPost("create-permission")]
        public async Task<ResponseDto> CreatePermission([FromBody]PermissionDto permissionDto)
        {
            return await _permissionService.CreatePermission(permissionDto);
        }
        
    }
}
