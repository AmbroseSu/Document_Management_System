using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IWorkflowService
{
    Task<ResponseDto> AddWorkflowAsync(WorkflowRequest workflowRequest);
    Task<ResponseDto> GetAllWorkflowAsync(string? workflowName, int page, int limit);
    Task<ResponseDto> UpdateWorkflowActiveOrDeleteAsync(Guid divisionId);
    Task<ResponseDto> GetWorkflowDetails(Guid divisionId);
    Task<ResponseDto> GetWorkflowDetailsWithFlowAndStep(Guid workflowId);
    Task<ResponseDto> CreateWorkflowAsync(CreateWorkFlowRequest workflowRequest);
    Task<ResponseDto> GetWorkflowByScopeAsync(Scope scope);
    Task<ResponseDto> FindMainWorkflowByScopeAsync(Scope scope);
}