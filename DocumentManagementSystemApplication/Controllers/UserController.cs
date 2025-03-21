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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpPost("create-user-by-form")]
        public async Task<ResponseDto> CreateUserByForm([FromBody] UserRequest userRequest)
        {
            return await _userService.CreateUserByForm(userRequest);
        }
    }
}
