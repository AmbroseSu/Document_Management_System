using System.Net;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using Newtonsoft.Json;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class WorkflowService : IWorkflowService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public WorkflowService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResponseDto> AddWorkflowAsync(WorkflowRequest workflowRequest)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (workflowRequest.WorkflowName == null)
            {
                return ResponseUtil.Error(ResponseMessages.WorkflowNameNotNull, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            
            var existingWorkflow  = await _unitOfWork.WorkflowUOW.FindWorkflowByNameAsync(workflowRequest.WorkflowName);
            if (existingWorkflow  != null)
            {
                return ResponseUtil.Error(ResponseMessages.WorkflowNameExisted, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            
            workflowRequest.WorkflowId ??= Guid.NewGuid();

            var workflow = new Workflow
            {
                WorkflowId = workflowRequest.WorkflowId.Value,
                WorkflowName = workflowRequest.WorkflowName,
                Scope = workflowRequest.Scope,
                IsAllocate = workflowRequest.IsAllocate,
                IsDeleted = false,
                CreateAt = DateTime.Now,
                CreateBy = workflowRequest.CreateBy,
            };
            
            if (workflowRequest.Scope.Equals(Scope.OutGoing))
            {
                workflow.RequiredRolesJson = JsonConvert.SerializeObject(new List<string> { "Leader", "Chief" });
            }
            else
            {
                if (workflowRequest.Scope.Equals(Scope.InComing))
                {
                    workflow.RequiredRolesJson = JsonConvert.SerializeObject(new List<string> { "Clerical Assistant", "Chief" });
                }
                else
                {
                    if (workflowRequest.Scope.Equals(Scope.School))
                    {
                        workflow.RequiredRolesJson = JsonConvert.SerializeObject(new List<string> { "Leader", "Chief" });
                    }
                }

            }
            
            
            await _unitOfWork.WorkflowUOW.AddAsync(workflow);
            await _unitOfWork.SaveChangesAsync();
            
            var workflowFlows = new List<WorkflowFlow>();
            var flowEntities = new List<Flow>();
            var stepEntities = new List<Step>();
            
            Dictionary<Guid, Guid> flowIdMapping = new(); // Map FlowId JSON -> FlowId DB
            Dictionary<Guid, Guid> stepIdMapping = new(); // Map StepId JSON -> StepId DB
            Dictionary<Guid, bool> flowFallbackFlags = new();  // Lưu trạng thái IsFallbackFlow tạm thời
            Dictionary<Guid, bool> stepFallbackFlags = new();  // Lưu trạng thái IsFallbackStep tạm thời
            
            foreach (var flowDto in workflowRequest.Flows!)
            {
                flowDto.FlowId ??= Guid.NewGuid();
                var flow = new Flow
                {
                    FlowId = flowDto.FlowId.Value,
                    // Không lưu IsFallbackFlow vào Flow mà chỉ xử lý trong Logic
                };
                flowEntities.Add(flow);
                flowIdMapping[flowDto.FlowId.Value] = flow.FlowId;

                // Lưu trạng thái IsFallbackFlow vào dictionary
                flowFallbackFlags[flow.FlowId] = flowDto.IsFallbackFlow ?? false;

                var workflowFlow = new WorkflowFlow
                {
                    WorkflowFlowId = Guid.NewGuid(),
                    WorkflowId = workflow.WorkflowId,
                    FlowId = flow.FlowId,
                    FlowNumber = workflowRequest.Flows.IndexOf(flowDto) + 1,
                    // Không lưu IsFallbackFlow vào WorkflowFlow mà xử lý logic sau
                };
                workflowFlows.Add(workflowFlow);
            }
            
            await _unitOfWork.FlowUOW.AddRangeAsync(flowEntities);
            await _unitOfWork.WorkflowFlowUOW.AddRangeAsync(workflowFlows);
            await _unitOfWork.SaveChangesAsync();
            
            // 4️⃣ Lưu Step và xử lý IsFallbackStep
            foreach (var flowDto in workflowRequest.Flows)
            {
                var stepNumbersInFlow = new HashSet<int>();
                foreach (var stepDto in flowDto.Steps!)
                {
                    if (stepNumbersInFlow.Contains(stepDto.StepNumber))
                    {
                        return ResponseUtil.Error(
                            $"StepNumber {stepDto.StepNumber} bị trùng trong Flow", 
                            ResponseMessages.OperationFailed, 
                            HttpStatusCode.BadRequest
                        );
                    }
                    stepNumbersInFlow.Add(stepDto.StepNumber);
                    stepDto.StepId ??= Guid.NewGuid();
                    var step = new Step
                    {
                        StepId = stepDto.StepId.Value,
                        StepNumber = stepDto.StepNumber,
                        Action = stepDto.Action,
                        FlowId = flowIdMapping[flowDto.FlowId.Value],
                        RoleId = stepDto.RoleId,
                        IsDeleted = false
                    };
                    stepEntities.Add(step);
                    stepIdMapping[stepDto.StepId.Value] = step.StepId;

                    // Lưu trạng thái IsFallbackStep vào dictionary
                    stepFallbackFlags[step.StepId] = stepDto.IsFallbackStep ?? false;
                }
            }
            
            await _unitOfWork.StepUOW.AddRangeAsync(stepEntities);
            await _unitOfWork.SaveChangesAsync();
            
            // 5️⃣ Cập nhật NextStepId và RejectStepId cho các bước
            foreach (var step in stepEntities)
            {
                var nextStep = stepEntities.FirstOrDefault(s => s.StepNumber == step.StepNumber + 1 && s.FlowId == step.FlowId);
                if (nextStep != null)
                {
                    step.NextStepId = nextStep.StepId;
                }

                // Xử lý RejectStepId khi bước hiện tại có IsFallbackStep = true
                if (stepFallbackFlags.TryGetValue(step.StepId, out bool isFallbackStep) && isFallbackStep)
                {
                    // Các bước tiếp theo sẽ có RejectStepId trỏ về bước fallback
                    foreach (var nextStepEntity in stepEntities.Where(s => s.StepNumber > step.StepNumber && s.FlowId == step.FlowId))
                    {
                        nextStepEntity.RejectStepId = step.StepId;
                    }
                }
            }
            
            await _unitOfWork.StepUOW.UpdateRangeAsync(stepEntities);
            //await _unitOfWork.SaveChangesAsync();
            
            var transitions = new List<WorkflowFlowTransition>();

            for (int i = 0; i < workflowFlows.Count - 1; i++)
            {
                var currentFlow = workflowFlows[i];
                var nextFlow = workflowFlows[i + 1];

                var transition = new WorkflowFlowTransition
                {
                    WorkflowFlowTransitionId = Guid.NewGuid(),
                    CurrentWorkFlowFlowId = currentFlow.WorkflowFlowId,
                    NextWorkFlowFlowId = nextFlow.WorkflowFlowId,
                    Condition = FlowTransitionCondition.Approve, // Gán điều kiện chuyển tiếp mặc định
                    IsDeleted = false
                };
                transitions.Add(transition);

                // Nếu flow hiện tại có IsFallbackFlow = true, tất cả các flow tiếp theo sẽ chuyển về flow này với Reject
                if (flowFallbackFlags.TryGetValue(currentFlow.FlowId, out bool isFallbackFlow) && isFallbackFlow)
                {
                    foreach (var nextWorkflowFlow in workflowFlows.Where(f => f.FlowId != currentFlow.FlowId))
                    {
                        var fallbackTransition = new WorkflowFlowTransition
                        {
                            WorkflowFlowTransitionId = Guid.NewGuid(),
                            CurrentWorkFlowFlowId = nextWorkflowFlow.WorkflowFlowId,
                            NextWorkFlowFlowId = currentFlow.WorkflowFlowId,
                            Condition = FlowTransitionCondition.Reject, // Chuyển về flow fallback với điều kiện Reject
                            IsDeleted = false
                        };
                        transitions.Add(fallbackTransition);
                    }
                }
            }
            
            if (transitions.Any())
            {
                await _unitOfWork.WorkflowFlowTransitionUOW.AddRangeAsync(transitions);
            }
            
            foreach (var (flowDto, index) in workflowRequest.Flows.Select((flow, idx) => (flow, idx)))
            {
                var firstStep = flowDto.Steps.OrderBy(s => s.StepNumber).FirstOrDefault();
                var lastStep = flowDto.Steps.OrderByDescending(s => s.StepNumber).FirstOrDefault();
                var allSteps = await _unitOfWork.StepUOW.FindAllStepAsync();

                // Kiểm tra xem có flow tiếp theo không để so sánh RoleEnd và RoleStart
                if (index < workflowRequest.Flows.Count - 1)
                {
                    var nextFlowDto = workflowRequest.Flows[index + 1];
                    var nextFirstStep = nextFlowDto.Steps.OrderBy(s => s.StepNumber).FirstOrDefault();

                    if (lastStep != null && nextFirstStep != null)
                    {
                        // Kiểm tra RoleEnd của flow hiện tại và RoleStart của flow tiếp theo
                        if (lastStep.RoleId != nextFirstStep.RoleId)
                        {
                            // Nếu không khớp, trả về lỗi
                            return ResponseUtil.Error(ResponseMessages.RoleEndCurrentFlowNotMatchRoleStartNextFlow, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                        }
                    }
                }

                if (firstStep != null && firstStep.RoleId != Guid.Empty)
                {
                    // Bao gồm thông tin Role
                    var firstStepWithRole = allSteps
                        .Where(s => s.StepId == firstStep.StepId)
                        .FirstOrDefault();

                    if (firstStepWithRole != null)
                    {
                        var flow = flowEntities.First(f => f.FlowId == flowIdMapping[flowDto.FlowId.Value]);
                        flow.RoleStart = firstStepWithRole.Role.RoleName; // Lấy tên role
                    }
                }

                if (lastStep != null && lastStep.RoleId != Guid.Empty)
                {
                    // Bao gồm thông tin Role
                    var lastStepWithRole = allSteps
                        .Where(s => s.StepId == lastStep.StepId)
                        .FirstOrDefault();

                    if (lastStepWithRole != null)
                    {
                        var flow = flowEntities.First(f => f.FlowId == flowIdMapping[flowDto.FlowId.Value]);
                        flow.RoleEnd = lastStepWithRole.Role.RoleName; // Lấy tên role
                    }
                }
            }


            await _unitOfWork.FlowUOW.UpdateRangeAsync(flowEntities);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return ResponseUtil.GetObject(workflowRequest, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    
    public async Task<ResponseDto> CreateWorkflowAsync(CreateWorkFlowRequest workflowRequest)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (workflowRequest.WorkflowName == null)
            {
                return ResponseUtil.Error(ResponseMessages.WorkflowNameNotNull, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            
            var existingWorkflow  = await _unitOfWork.WorkflowUOW.FindWorkflowByNameAsync(workflowRequest.WorkflowName);
            if (existingWorkflow  != null)
            {
                return ResponseUtil.Error(ResponseMessages.WorkflowNameExisted, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            

            var workflow = new Workflow
            {
                WorkflowId = Guid.NewGuid(),
                WorkflowName = workflowRequest.WorkflowName,
                Scope = workflowRequest.Scope,
                IsAllocate = true,
                IsDeleted = false,
                CreateAt = DateTime.Now,
                CreateBy = workflowRequest.CreateBy,
            };
            
            if (workflowRequest.Scope.Equals(Scope.OutGoing))
            {
                workflow.RequiredRolesJson = JsonConvert.SerializeObject(new List<string> { "Leader", "Chief" });
            }
            else
            {
                if (workflowRequest.Scope.Equals(Scope.InComing))
                {
                    workflow.RequiredRolesJson = JsonConvert.SerializeObject(new List<string> { "Clerical Assistant", "Chief" });
                }
                else
                {
                    if (workflowRequest.Scope.Equals(Scope.School))
                    {
                        workflow.RequiredRolesJson = JsonConvert.SerializeObject(new List<string> { "Leader", "Chief" });
                    }
                }

            }
            
            await _unitOfWork.WorkflowUOW.AddAsync(workflow);
            await _unitOfWork.SaveChangesAsync();

            var workflowFlows = new List<WorkflowFlow>();
            var documentTypeWorkflows = new List<DocumentTypeWorkflow>();
            var flowEntities = new List<Flow>();
            
            
            foreach (var flowId in workflowRequest.FlowIds)
            {
                var flowEntity = await _unitOfWork.FlowUOW.FindFlowByIdAsync(flowId);
                flowEntities.Add(flowEntity);
                var workflowFlow = new WorkflowFlow
                {
                    WorkflowFlowId = Guid.NewGuid(),
                    WorkflowId = workflow.WorkflowId,
                    FlowId = flowId,
                    FlowNumber = workflowRequest.FlowIds.IndexOf(flowId) + 1,
                    // Không lưu IsFallbackFlow vào WorkflowFlow mà xử lý logic sau
                };
                workflowFlows.Add(workflowFlow);
            }
            
            

            await _unitOfWork.WorkflowFlowUOW.AddRangeAsync(workflowFlows);
            await _unitOfWork.SaveChangesAsync();

            foreach (var documentTypeId in workflowRequest.DocumentTypeIds)
            {
                var documentTypeWorkflow = new DocumentTypeWorkflow
                {
                    DocumentTypeWorkflowId = Guid.NewGuid(),
                    WorkflowId = workflow.WorkflowId,
                    DocumentTypeId = documentTypeId
                };
                documentTypeWorkflows.Add(documentTypeWorkflow);
                
            }

            await _unitOfWork.DocumentTypeWorkflowUOW.AddRangeAsync(documentTypeWorkflows);
            await _unitOfWork.SaveChangesAsync();

            for (int i = 0; i < flowEntities.Count - 1; i++)
            {
                var lastStep = flowEntities[i].Steps.OrderByDescending(s => s.StepNumber).FirstOrDefault();
                var nextFlow = flowEntities[i + 1];
                var nextFirstStep = nextFlow.Steps.OrderBy(s => s.StepNumber).FirstOrDefault();
                if (lastStep != null && nextFirstStep != null)
                {
                    // Kiểm tra RoleEnd của flow hiện tại và RoleStart của flow tiếp theo
                    if (lastStep.RoleId != nextFirstStep.RoleId)
                    {
                        // Nếu không khớp, trả về lỗi
                        return ResponseUtil.Error(ResponseMessages.RoleEndCurrentFlowNotMatchRoleStartNextFlow, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                    }
                }
                
                
            }
            
            var requiredRoles = JsonConvert.DeserializeObject<List<string>>(workflow.RequiredRolesJson);
            bool hasValidFlow = false;

            foreach (var flow in flowEntities)
            {
                // var steps = (await _unitOfWork.StepUOW.FindStepByFlowIdAsync(flow.FlowId)).ToList();
                // if (steps.Count >= 2)
                // {
                    var startRole = flow.RoleStart;
                    var endRole = flow.RoleEnd;

                    if (requiredRoles.First().ToLower().Equals(startRole.ToLower()) && requiredRoles.Last().ToLower().Equals(endRole.ToLower()))
                    {
                        hasValidFlow = true;
                        break;
                    }
                //}
            }

            if (!hasValidFlow)
            {
                return ResponseUtil.Error($"Workflow phải có 2 vai trò chính {requiredRoles.First()} và {requiredRoles.Last()}", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return ResponseUtil.GetObject(workflowRequest, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    
    
    
    
    public async Task<ResponseDto> UpdateWorkflowActiveOrDeleteAsync(Guid workflowId)
    {
        try
        {
            if (workflowId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.WorkflowIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
            if (workflow == null)
                return ResponseUtil.Error(ResponseMessages.WorkflowNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            

            if (workflow.IsDeleted)
            {
                workflow.IsDeleted = false;
                await _unitOfWork.WorkflowUOW.UpdateAsync(workflow);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.GetObject(ResponseMessages.WorkflowActive, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 1);
            }

            workflow.IsDeleted = true;
            await _unitOfWork.WorkflowUOW.UpdateAsync(workflow);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.WorkflowHasDeleted, ResponseMessages.DeleteSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetAllWorkflowAsync(string? workflowName, int page, int limit)
    {
        try
        {
            var workflows = await _unitOfWork.WorkflowUOW.FindAllWorkflowAsync();
            
            if (!string.IsNullOrWhiteSpace(workflowName))
            {
                workflows = workflows
                    .Where(w => w.WorkflowName.ToLower().Contains(workflowName.ToLower()))
                    .ToList();
            }
            
            var totalRecords = workflows.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / limit);

            IEnumerable<Workflow> workflowResults = workflows
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            var result = _mapper.Map<IEnumerable<Workflow>>(workflows);
            return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetWorkflowDetails(Guid workflowId)
    {
        try
        {
            if (workflowId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.WorkflowIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
            if (workflow == null)
                return ResponseUtil.Error(ResponseMessages.WorkflowNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            var result = _mapper.Map<WorkflowDto>(workflow);
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetWorkflowDetailsWithFlowAndStep(Guid workflowId)
    {
        try
        {
            if (workflowId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.WorkflowIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
            if (workflow == null)
                return ResponseUtil.Error(ResponseMessages.WorkflowNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var workflowFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var workflowFlowIds = workflowFlows.Select(wf => wf.WorkflowFlowId).ToList();
            
            var transitions = await _unitOfWork.WorkflowFlowTransitionUOW.FindByWorkflowFlowIdsAsync(workflowFlowIds);
            
            var flowIds = workflowFlows.Select(wf => wf.FlowId).ToList();
            var flows = await _unitOfWork.FlowUOW.FindByIdsAsync(flowIds);
            
            var steps = await _unitOfWork.StepUOW.FindByFlowIdsAsync(flowIds);
            
            var flowDtoList = workflowFlows
                .OrderBy(wf => wf.FlowNumber) // 🔹 Thêm dòng này để sắp xếp
                .Select(workflowFlow =>
                {
                    var isFallbackFlow = transitions.Any(t =>
                        t.NextWorkFlowFlowId == workflowFlow.WorkflowFlowId &&
                        t.Condition == FlowTransitionCondition.Reject
                    );

                    var flow = flows.FirstOrDefault(f => f.FlowId == workflowFlow.FlowId);
                    if (flow == null) return null;

                    var stepsInFlow = steps.Where(s => s.FlowId == flow.FlowId).ToList();

                    return new FlowDto
                    {
                        FlowId = flow.FlowId,
                        IsFallbackFlow = isFallbackFlow,
                        RoleStart = flow.RoleStart,
                        RoleEnd = flow.RoleEnd,
                        Steps = stepsInFlow
                            .OrderBy(s => s.StepNumber) // 🔹 Nếu cần sắp xếp steps theo StepNumber
                            .Select(s => new StepDto
                            {
                                StepId = s.StepId,
                                StepNumber = s.StepNumber,
                                Action = s.Action,
                                RoleId = s.RoleId,
                                Role = new RoleDto
                                {
                                    RoleId = s.RoleId,
                                    RoleName = s.Role?.RoleName,
                                    CreatedDate = s.Role?.CreatedDate
                                },
                                NextStepId = s.NextStepId,
                                RejectStepId = s.RejectStepId,
                                IsFallbackStep = s.RejectStepId == null, // ✅ Gán giá trị ở đây thay vì sửa trong entity
                                TaskDtos = new List<TaskDto>() // Nếu có TaskId thì lấy từ bảng liên quan
                            }).ToList()
                    };
                })
                .Where(flowDto => flowDto != null) // ✅ Loại bỏ flow null
                .ToList();
            

            var workflowResponse = new WorkflowRequest
            {
                WorkflowId = workflow.WorkflowId,
                WorkflowName = workflow.WorkflowName,
                RequiredRolesJson = workflow.RequiredRolesJson,
                Scope = workflow.Scope,
                IsAllocate = workflow.IsAllocate,
                CreateBy = workflow.CreateBy ?? Guid.Empty,
                Flows = flowDtoList
            };
            return ResponseUtil.GetObject(workflowResponse, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
    
    public async Task<ResponseDto> GetWorkflowByScopeAsync(Scope scope)
    {
        try
        {
            var workflows = await _unitOfWork.WorkflowUOW.FindAllWorkflowByScopeAsync(scope);
            if (workflows == null)
                return ResponseUtil.Error(ResponseMessages.WorkflowNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            
            var workflowResponses = new List<WorkflowScopeResponse>();
            foreach (var workflow in workflows)
            {
                var documentTypeWorkflows = await _unitOfWork.DocumentTypeWorkflowUOW.FindAllDocumentTypeNameByWorkflowIdAsync(workflow.WorkflowId);
                var documentTypeDtos = documentTypeWorkflows.Select(dt => _mapper.Map<DocumentTypeDto>(dt.DocumentType)).ToList();
                
                workflowResponses.Add(new WorkflowScopeResponse
                {
                    WorkflowId = workflow.WorkflowId,
                    WorkflowName = workflow.WorkflowName,
                    DocumentTypes = documentTypeDtos
                });
                
            }
            
            return ResponseUtil.GetObject(workflowResponses, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
}