using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var userId = User.FindFirst("userid")?.Value;
            return await _userDocPermissionService.GrantPermissionForDocument(Guid.Parse(userId), grantDocumentRequest);
        }
        
    }
}
