using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FlowController : ControllerBase
    {
        private readonly IFlowService _flowService;

        public FlowController(IFlowService flowService)
        {
            _flowService = flowService;
        }
        [HttpGet("view-all-flow")]
        //[AuthorizeResource("[Flow] View All Flow")]
        public async Task<ResponseDto> ViewAllFlow()
        {
            return await _flowService.FindAllFlowAsync();
        }
    }
}
