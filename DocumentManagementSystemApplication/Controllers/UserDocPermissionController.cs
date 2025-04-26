using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDocPermissionController : ControllerBase
    {
        private readonly IUserDocPermissionService _userDocPermissionService;

        public UserDocPermissionController(IUserDocPermissionService userDocPermissionService)
        {
            _userDocPermissionService = userDocPermissionService;
        }

        [HttpPost("create-grand-permission-for-document")]
        public async Task<ResponseDto> CreateGrandPermissionForDocument(
            [FromBody] GrantDocumentRequest grantDocumentRequest)
        {
            return await _userDocPermissionService.GrantPermissionForDocument(grantDocumentRequest);
        }
        
    }
}
