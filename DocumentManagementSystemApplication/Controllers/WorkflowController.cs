using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }
        
        [HttpPost("create-workflow")]
        public async Task<ResponseDto> CreateWorkflow([FromBody] WorkflowRequest workflowRequest)
        {
            return await _workflowService.AddWorkflowAsync(workflowRequest);    
        }
        
        [HttpGet("view-all-workflow")]
        public async Task<ResponseDto> ViewAllWorkflow([FromQuery] int page = 1,[FromQuery] int limit = 10)
        {
            return await _workflowService.GetAllWorkflowAsync(page, limit);
        }
        
        [HttpPost("delete-workflow")]
        public async Task<ResponseDto> DeleteWorkflow([FromQuery] Guid workflowId)
        {
            return await _workflowService.UpdateWorkflowActiveOrDeleteAsync(workflowId);
        }
        
        [HttpGet("view-workflow-details")]
        public async Task<ResponseDto> ViewWorkflowDetails([FromQuery] Guid workflowId)
        {
            return await _workflowService.GetWorkflowDetails(workflowId);
        }
        
        [HttpGet("view-workflow-details-with-flow-and-step")]
        public async Task<ResponseDto> ViewWorkflowDetailsWithFlowAndStep([FromQuery] Guid workflowId)
        {
            return await _workflowService.GetWorkflowDetailsWithFlowAndStep(workflowId);
        }
    }
}
