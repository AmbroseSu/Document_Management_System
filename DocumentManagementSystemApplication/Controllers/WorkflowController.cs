using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;
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
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }
        
        [HttpPost("create-workflow")]
        [AuthorizeResource("[Workflow] Create Workflow")]
        public async Task<ResponseDto> CreateWorkflow([FromBody] CreateWorkFlowRequest workflowRequest)
        {
            return await _workflowService.CreateWorkflowAsync(workflowRequest);    
        }
        
        [HttpGet("view-all-workflow")]
        [AuthorizeResource("[Workflow] View All Workflow")]
        public async Task<ResponseDto> ViewAllWorkflow([FromQuery] string? workflowName, [FromQuery] Scope? scope, [FromQuery] int page = 1,[FromQuery] int limit = 10)
        {
            return await _workflowService.GetAllWorkflowAsync(workflowName, scope, page, limit);
        }
        
        [HttpPost("delete-workflow")]
        [AuthorizeResource("[Workflow] Delete Workflow")]
        public async Task<ResponseDto> DeleteWorkflow([FromQuery] Guid workflowId)
        {
            return await _workflowService.UpdateWorkflowActiveOrDeleteAsync(workflowId);
        }
        
        [HttpGet("view-workflow-details")]
        [AuthorizeResource("[Workflow] View Workflow Details")]
        public async Task<ResponseDto> ViewWorkflowDetails([FromQuery] Guid workflowId)
        {
            return await _workflowService.GetWorkflowDetails(workflowId);
        }
        
        [HttpGet("view-workflow-details-with-flow-and-step")]
        [AuthorizeResource("[Workflow] View Workflow Details With Flow And Step")]
        public async Task<ResponseDto> ViewWorkflowDetailsWithFlowAndStep([FromQuery] Guid workflowId)
        {
            return await _workflowService.GetWorkflowDetailsWithFlowAndStep(workflowId);
        }

        [HttpGet("view-workflow-by-scope")]
        [AuthorizeResource("[Workflow] View Workflow By Scope")]
        public async Task<ResponseDto> ViewWorkflowByScope([FromQuery] Scope scope)
        {
            return await _workflowService.GetWorkflowByScopeAsync(scope);
        }

        [HttpGet("view-main-workflow-by-scope")]
        [AuthorizeResource("[Workflow] View Main Workflow By Scope")]
        public async Task<ResponseDto> ViewMainWorkflowByScope([FromQuery] Scope scope)
        {
            return await _workflowService.FindMainWorkflowByScopeAsync(scope);
        }
    }
}
