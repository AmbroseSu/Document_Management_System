using DataAccess.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly ILoggingService _logService;

        public LogController(ILoggingService logService)
        {
            _logService = logService;
        }

        [HttpGet("view-all-log")]
        public async Task<ResponseDto> ViewAllLog([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await _logService.GetAllLogsAsync(page, pageSize);
        }
    }
}