using BusinessObject.Enums;
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
        public async Task<ResponseDto> CreateWorkflow([FromBody] CreateWorkFlowRequest workflowRequest)
        {
            return await _workflowService.CreateWorkflowAsync(workflowRequest);    
        }
        
        [HttpGet("view-all-workflow")]
        public async Task<ResponseDto> ViewAllWorkflow([FromQuery] string? workflowName, [FromQuery] int page = 1,[FromQuery] int limit = 10)
        {
            return await _workflowService.GetAllWorkflowAsync(workflowName, page, limit);
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

        [HttpGet("view-workflow-by-scope")]
        public async Task<ResponseDto> ViewWorkflowByScope([FromQuery] Scope scope)
        {
            return await _workflowService.GetWorkflowByScopeAsync(scope);
        }
    }
}
