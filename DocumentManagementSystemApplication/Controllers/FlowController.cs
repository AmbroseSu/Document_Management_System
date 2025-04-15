using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowController : ControllerBase
    {
        private readonly IFlowService _flowService;

        public FlowController(IFlowService flowService)
        {
            _flowService = flowService;
        }
        [HttpGet("view-all-flow")]
        public async Task<ResponseDto> ViewAllFlow()
        {
            return await _flowService.FindAllFlowAsync();
        }
    }
}
