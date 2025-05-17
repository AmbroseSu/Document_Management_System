using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IWorkflowService
{
    Task<ResponseDto> AddWorkflowAsync(WorkflowRequest workflowRequest);
    Task<ResponseDto> GetAllWorkflowAsync(string? workflowName, Scope? scope, int page, int limit);
    Task<ResponseDto> UpdateWorkflowActiveOrDeleteAsync(Guid divisionId, Guid userId);
    Task<ResponseDto> GetWorkflowDetails(Guid divisionId);
    Task<ResponseDto> GetWorkflowDetailsWithFlowAndStep(Guid workflowId);
    Task<ResponseDto> CreateWorkflowAsync(CreateWorkFlowRequest workflowRequest, Guid userId);
    Task<ResponseDto> GetWorkflowByScopeAsync(Scope scope);
    Task<ResponseDto> FindMainWorkflowByScopeAsync(Scope scope);
}