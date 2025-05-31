using System.Linq.Dynamic;
using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Option;
using DataAccess;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Repository;
using Service.Response;
using Service.SignalRHub;
using Service.Utilities;

namespace Service.Impl;

public partial class TaskService : ITaskService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly MongoDbService _notificationCollection;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IFileService _fileService;
    private readonly IDocumentService _documentService;
    private readonly string _host;
    private readonly ILoggingService _loggingService;
    

    public TaskService(IMapper mapper, IUnitOfWork unitOfWork, INotificationService notificationService,
        MongoDbService notificationCollection, IHubContext<NotificationHub> hubContext, IFileService fileService,
        IDocumentService documentService,IOptions<AppsetingOptions> options, ILoggingService loggingService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _notificationCollection = notificationCollection;
        _hubContext = hubContext;
        _fileService = fileService;
        _documentService = documentService;
        _loggingService = loggingService;
        _host = options.Value.Host;

    }

    public async Task<ResponseDto> CreateTaskFix(Guid userId, TaskDto taskDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskDto.Title) || string.IsNullOrWhiteSpace(taskDto.Description))
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.StepId == null)
                return ResponseUtil.Error(ResponseMessages.StepIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.UserId == null)
                return ResponseUtil.Error(ResponseMessages.UserIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.DocumentId == null)
                return ResponseUtil.Error(ResponseMessages.DocumentIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);


            if (taskDto.StartDate < DateTime.Now || taskDto.EndDate < DateTime.Now)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (taskDto.EndDate <= taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var existingTasks =
                await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(taskDto.StepId, taskDto.DocumentId);

            var nextTaskNumber = existingTasks.Count() + 1;


            var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(taskDto.StepId);
            if (currentStep.Role.RoleName.Equals("chief", StringComparison.OrdinalIgnoreCase))
            {
                var signTasksInStep = existingTasks.Where(t => t.TaskType == TaskType.Sign).ToList();
                if (signTasksInStep.Any() && taskDto.TaskType == TaskType.Sign)
                {
                    return ResponseUtil.Error(
                        ResponseMessages.SignExistNotCreate,
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }

            var currentFlow = currentStep!.FlowId;

            var workflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByFlowIdAsync(currentFlow);

            var workflowId = workflowFlow!.WorkflowId;


            var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var firstFlowInWorkflow = workflowFlowAll!.OrderBy(wf => wf.FlowNumber).FirstOrDefault();

            var stepAllOfFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(firstFlowInWorkflow!.FlowId);
            var firstStepInFlow = stepAllOfFlow!.OrderBy(s => s.StepNumber).FirstOrDefault();

            var isCreateSubmit = await IsSubmitAllowedInFirstTaskOfNonFirstFlow(currentFlow, currentStep.StepNumber,
                nextTaskNumber, taskDto.TaskType, taskDto.UserId.Value);

            if (!isCreateSubmit)
            {
                return ResponseUtil.Error(ResponseMessages.CanCreateTaskSubmit, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(taskDto.UserId.Value);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            
            var userRoles = user.UserRoles
                .Select(ur => ur.Role.RoleName)
                .ToList();
            var primaryRoles = userRoles
                .Select(name =>
                {
                    // Nếu role có chứa "_", lấy phần cuối sau dấu "_"
                    if (name.Contains("_"))
                    {
                        var parts = name.Split('_');
                        return parts[^1].Trim().ToLower(); // phần cuối cùng
                    }

                    return name.Trim().ToLower();
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var matchedRoles = primaryRoles.Where(role => role.Equals(currentStep.Role.RoleName.ToLower())).ToList();
            if (!matchedRoles.Any())
            {
                return ResponseUtil.Error(ResponseMessages.UserNotRoleWithStep, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }



            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(taskDto.DocumentId.Value);
            var orderedTasks = await GetOrderedTasks(document.Tasks.Where(t => t.IsDeleted == false).ToList(),
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            if (orderedTasks.Count == 0)
                return ResponseUtil.Error(ResponseMessages.TaskFirstNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (orderedTasks[orderedTasks.Count - 1].EndDate > taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayLowerEndDaypreviousStepFailed,
                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            if (orderedTasks[0].TaskStatus == TasksStatus.Completed)
            {
                var taskOfUser = orderedTasks.Where(t => t.UserId == userId && t.TaskType == TaskType.Create).ToList();
                if (taskOfUser.Any())
                {
                }
                else
                {
                    return ResponseUtil.Error(ResponseMessages.TaskCanNotCreate, ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }

            // neu task của người tạo là create và đã complete thì kkhoogn được tạo task nua
            var createTasks = orderedTasks.Where(t => t.TaskType == TaskType.Create).ToList();
            if (createTasks.Any() && taskDto.TaskType == TaskType.Create)
            {
                return ResponseUtil.Error(
                    ResponseMessages.OnlyOneCreateTaskAllowed,
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            if (taskDto.StepId == firstStepInFlow!.StepId && orderedTasks.Count == 0)
            {
                taskDto.TaskStatus = TasksStatus.InProgress;
                // if (taskDto.DocumentId == null)
                // {
                //     return ResponseUtil.Error(ResponseMessages.DocumentIdNull, ResponseMessages.OperationFailed,
                //         HttpStatusCode.BadRequest);
                // }
            }
            else
            {
                taskDto.TaskStatus = TasksStatus.Waiting;
            }

            taskDto.TaskNumber = nextTaskNumber;
            taskDto.CreatedDate = DateTime.Now;
            taskDto.IsDeleted = false;
            taskDto.IsActive = true;
            var userCreate = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            taskDto.TaskId = Guid.NewGuid();
            var task = new Tasks
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                StartDate = taskDto.StartDate,
                EndDate = taskDto.EndDate,
                TaskStatus = taskDto.TaskStatus ?? TasksStatus.Waiting,
                TaskType = taskDto.TaskType,
                CreatedDate = DateTime.Now,
                TaskNumber = taskDto.TaskNumber,
                IsDeleted = taskDto.IsDeleted ?? false,
                IsActive = taskDto.IsActive ?? true,
                StepId = taskDto.StepId ?? Guid.Empty,
                DocumentId = taskDto.DocumentId ?? null,
                UserId = taskDto.UserId ?? Guid.Empty,
                CreatedBy = userCreate!.FullName,
            };
            await _unitOfWork.TaskUOW.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(taskDto, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ResponseDto> CreateTask(Guid userId, TaskDto taskDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskDto.Title) || string.IsNullOrWhiteSpace(taskDto.Description))
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.StepId == null)
                return ResponseUtil.Error(ResponseMessages.StepIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.UserId == null)
                return ResponseUtil.Error(ResponseMessages.UserIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.DocumentId == null)
                return ResponseUtil.Error(ResponseMessages.DocumentIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);



            var existingTasks =
                await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(taskDto.StepId, taskDto.DocumentId);

            var nextTaskNumber = (taskDto.TaskNumber > 0) ? taskDto.TaskNumber : (existingTasks.Count() + 1);
            //var nextTaskNumber = existingTasks.Count() + 1;

            var isTaskNumberDuplicated = existingTasks.Any(t => t.TaskNumber == nextTaskNumber);
            if (isTaskNumberDuplicated)
            {
                return ResponseUtil.Error("TaskNumber đã tồn tại trong Step này. Vui lòng chọn TaskNumber khác.",
                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            /////////////////////////////////
            if (existingTasks.Any())
            {
                var existingUserIds = existingTasks.Select(t => t.UserId).Distinct().ToList();
                if (existingUserIds.Any() && !existingUserIds.Contains(taskDto.UserId.Value))
                {
                    return ResponseUtil.Error(
                        "Step này đã có task của người dùng khác. Chỉ một người dùng được phép có task trong cùng step.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            
            foreach (var t in existingTasks)
            {
                if (t.TaskType == taskDto.TaskType && !t.IsDeleted)
                {
                    return ResponseUtil.Error(
                        "Mỗi loại nhiệm vụ chỉ được phép xuất hiện một lần trong Step.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            
            if (taskDto.TaskType == TaskType.Browse && existingTasks.Count() > 0)
            {
                foreach (var t in existingTasks)
                {
                    if (t.TaskNumber < nextTaskNumber && t.TaskType == TaskType.Submit && !t.IsDeleted)
                    {
                        return ResponseUtil.Error(
                            $"Không thể tạo nhiệm vụ Duyệt vì đã có nhiệm vụ Nộp trước đó trong Bước này. Nhiệm vụ Duyệt Phải trước nhiệm vụ Nộp.",
                            ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);
                    }
                }
            }
            
            
            // Kiểm tra khi tạo task Sign, phải có Browse trước đó trong cùng Step
            if (taskDto.TaskType == TaskType.Sign)
            {
                var hasBrowseOrSubmitBefore = false;
                foreach (var t in existingTasks)
                {
                    if (t.TaskNumber < nextTaskNumber && t.TaskType == TaskType.Browse && !t.IsDeleted)
                    {
                        hasBrowseOrSubmitBefore = true;
                        break;
                    }
                }
                if (!hasBrowseOrSubmitBefore)
                {
                    return ResponseUtil.Error(
                        $"Không thể tạo task Sign vì không có task Browse trước đó trong Step này.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            


            var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(taskDto.StepId);
            if (currentStep.Role.RoleName.Equals("chief", StringComparison.OrdinalIgnoreCase))
            {
                var signTasksInStep = existingTasks.Where(t => t.TaskType == TaskType.Sign).ToList();
                if (signTasksInStep.Any() && taskDto.TaskType == TaskType.Sign)
                {
                    return ResponseUtil.Error(
                        ResponseMessages.SignExistNotCreate,
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            
            
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(taskDto.DocumentId.Value);

            var currentFlow = currentStep!.FlowId;

            var workflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAndFlowIdAsync(document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId, currentFlow);

            var workflowId = workflowFlow!.WorkflowId;


            var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var firstFlowInWorkflow = workflowFlowAll!.OrderBy(wf => wf.FlowNumber).FirstOrDefault();

            var stepAllOfFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(firstFlowInWorkflow!.FlowId);
            var firstStepInFlow = stepAllOfFlow!.OrderBy(s => s.StepNumber).FirstOrDefault();

            var isCreateSubmit = await IsSubmitAllowedInFirstTaskOfNonFirstFlow(currentFlow, currentStep.StepNumber,
                nextTaskNumber, taskDto.TaskType, taskDto.UserId.Value);

            if (!isCreateSubmit)
            {
                return ResponseUtil.Error(ResponseMessages.CanCreateTaskSubmit, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            
            var currentFlowNumber = workflowFlowAll
                .FirstOrDefault(wf => wf.FlowId == currentFlow)?.FlowNumber ?? 0;
            
            //////////////////////////////////////////////
            
        //    var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(taskDto.StepId);
        //var currentFlow = currentStep!.FlowId;
        var stepsInFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(currentFlow);
        var orderedStepsInFlowNew = new List<Step>();
        foreach (var s in stepsInFlow)
        {
            orderedStepsInFlowNew.Add(s);
        }
        for (int i = 0; i < orderedStepsInFlowNew.Count - 1; i++)
        {
            for (int j = i + 1; j < orderedStepsInFlowNew.Count; j++)
            {
                if (orderedStepsInFlowNew[j].StepNumber < orderedStepsInFlowNew[i].StepNumber)
                {
                    var temp = orderedStepsInFlowNew[i];
                    orderedStepsInFlowNew[i] = orderedStepsInFlowNew[j];
                    orderedStepsInFlowNew[j] = temp;
                }
            }
        }
        if (orderedStepsInFlowNew.Count > 0 && taskDto.StepId == orderedStepsInFlowNew[0].StepId)
        {
            //var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(taskDto.DocumentId.Value);
            //var workflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAndFlowIdAsync(
            //    document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId, currentFlow);
            //var workflowId = workflowFlow!.WorkflowId;
            //var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            //var currentFlowNumber = 0;
            foreach (var wf in workflowFlowAll)
            {
                if (wf.FlowId == currentFlow)
                {
                    currentFlowNumber = wf.FlowNumber;
                    break;
                }
            }
            if (currentFlowNumber > 1)
            {
                WorkflowFlow previousFlow = null;
                foreach (var wf in workflowFlowAll)
                {
                    if (wf.FlowNumber == currentFlowNumber - 1)
                    {
                        previousFlow = wf;
                        break;
                    }
                }
                if (previousFlow != null)
                {
                    var stepsInPreviousFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(previousFlow.FlowId);
                    var orderedStepsInPreviousFlow = new List<Step>();
                    foreach (var s in stepsInPreviousFlow)
                    {
                        orderedStepsInPreviousFlow.Add(s);
                    }
                    for (int i = 0; i < orderedStepsInPreviousFlow.Count - 1; i++)
                    {
                        for (int j = i + 1; j < orderedStepsInPreviousFlow.Count; j++)
                        {
                            if (orderedStepsInPreviousFlow[j].StepNumber < orderedStepsInPreviousFlow[i].StepNumber)
                            {
                                var temp = orderedStepsInPreviousFlow[i];
                                orderedStepsInPreviousFlow[i] = orderedStepsInPreviousFlow[j];
                                orderedStepsInPreviousFlow[j] = temp;
                            }
                        }
                    }
                    var lastStepInPreviousFlow = orderedStepsInPreviousFlow.Count > 0 ? orderedStepsInPreviousFlow[orderedStepsInPreviousFlow.Count - 1] : null;
                    if (lastStepInPreviousFlow != null)
                    {
                        var tasksInLastStep = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(lastStepInPreviousFlow.StepId, taskDto.DocumentId);
                        foreach (var t in tasksInLastStep)
                        {
                            if (t.TaskType == taskDto.TaskType && !t.IsDeleted)
                            {
                                return ResponseUtil.Error(
                                    $"Step cuối của Flow trước đó đã có task với loại {taskDto.TaskType}. Mỗi loại TaskType chỉ được phép xuất hiện một lần.",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }
                        }
                        // Kiểm tra khi tạo task Sign, Step cuối của Flow trước đó phải có Browse hoặc Submit
                        if (taskDto.TaskType == TaskType.Sign)
                        {
                            var hasBrowseOrSubmit = false;
                            foreach (var t in tasksInLastStep)
                            {
                                if (t.TaskType == TaskType.Browse && !t.IsDeleted)
                                {
                                    hasBrowseOrSubmit = true;
                                    break;
                                }
                            }
                            if (!hasBrowseOrSubmit)
                            {
                                return ResponseUtil.Error(
                                    $"Không thể tạo task Sign vì Step cuối của Flow trước đó không có task Browse",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }
                        }
                    }
                }
            }
        }
            
            
            
            ////////////////////////////////////////////

            
            if (taskDto.StartDate < DateTime.Now || taskDto.EndDate < DateTime.Now)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayEndDayFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
            if (taskDto.EndDate <= taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
            


            ////////////////////////////////////////////////////////////////////////

            // Lấy tất cả Step trong Flow này
            //var stepsInFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(currentFlow);
            var orderedStepsInFlow = stepsInFlow.OrderBy(s => s.StepNumber).ToList();

            // Lấy các task trong Step hiện tại và sắp xếp theo TaskNumber
            var tasksInCurrentStep =
                await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(taskDto.StepId, taskDto.DocumentId);
            var orderedTasksInCurrentStep = tasksInCurrentStep.OrderBy(t => t.TaskNumber).ToList();
            
            var currentStepIndex = orderedStepsInFlow.FindIndex(s => s.StepId == taskDto.StepId);
            
            
            
            
                    // Kiểm tra nếu task mới là task đầu tiên của step đầu tiên trong flow và có flow trước đó
        if (!tasksInCurrentStep.Any() && currentStepIndex == 0) // Task đầu tiên của step đầu tiên
        {
            // var currentFlowNumber = workflowFlowAll
            //     .FirstOrDefault(wf => wf.FlowId == currentFlow)?.FlowNumber ?? 0;

            if (currentFlowNumber > 1) // Có flow trước đó
            {
                var previousFlow = workflowFlowAll
                    .Where(wf => wf.FlowNumber == currentFlowNumber - 1)
                    .FirstOrDefault();

                if (previousFlow != null)
                {
                    var stepsInPreviousFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(previousFlow.FlowId);
                    var lastStepInPreviousFlow = stepsInPreviousFlow.OrderBy(s => s.StepNumber).LastOrDefault();

                    if (lastStepInPreviousFlow != null)
                    {
                        var tasksInLastStep = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(lastStepInPreviousFlow.StepId, taskDto.DocumentId);
                        var lastTaskInPreviousFlow = tasksInLastStep.OrderBy(t => t.TaskNumber).LastOrDefault();

                        if (lastTaskInPreviousFlow != null)
                        {
                            // Kiểm tra nếu task cuối của flow trước đó là Submit
                            if (lastTaskInPreviousFlow.TaskType == TaskType.Submit)
                            {
                                return ResponseUtil.Error(
                                    "Không thể tạo nhiệm vụ mới vì nhiệm cuối cùng của luồng trước đó là Nộp văn bản.",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }

                            // Kiểm tra UserId nếu không phải Submit
                            if (lastTaskInPreviousFlow.UserId != taskDto.UserId.Value)
                            {
                                return ResponseUtil.Error(
                                    "Người thực hiện không trùng khớp với nhiệm vụ trước.",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }
                        }
                    }
                }
            }
        }
                
                if (currentStepIndex > 0) // Đảm bảo có step trước đó
                {
                    var previousStepCheckTask = orderedStepsInFlow[currentStepIndex - 1];
                    var tasksInPreviousStep =
                        await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(previousStepCheckTask.StepId, taskDto.DocumentId);
                    var lastTaskInPreviousStep = tasksInPreviousStep.OrderBy(t => t.TaskNumber).LastOrDefault();
                    if (lastTaskInPreviousStep != null)
                    {
                        if (taskDto.StartDate <= lastTaskInPreviousStep.EndDate)
                        {
                            return ResponseUtil.Error(
                                "StartDate của task mới phải lớn hơn EndDate của task cuối cùng trong step trước đó.",
                                ResponseMessages.OperationFailed,
                                HttpStatusCode.BadRequest);
                        }
                    }
                }
                
                if (currentStepIndex >= 0 && currentStepIndex < orderedStepsInFlow.Count - 1)
                {
                    var nextStep = orderedStepsInFlow[currentStepIndex + 1];
                    var firstTaskInNextStep =
                        await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(nextStep.StepId, taskDto.DocumentId);
                    var firstTaskInNextStepOrdered = firstTaskInNextStep.OrderBy(t => t.TaskNumber).FirstOrDefault();
                    if (firstTaskInNextStepOrdered != null)
                    {
                        if (taskDto.EndDate >= firstTaskInNextStepOrdered.StartDate)
                        {
                            return ResponseUtil.Error(ResponseMessages.TaskEndDateAfterNextStepStartDate,
                                ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                        }
                    }
                }

            // Kiểm tra giữa các Step trong Flow
            var lastTaskInCurrentStep = orderedTasksInCurrentStep.LastOrDefault();
            if (lastTaskInCurrentStep != null)
            {
                // Kiểm tra StartDate của task mới với EndDate của task cuối cùng của Step hiện tại
                if (taskDto.StartDate <= lastTaskInCurrentStep.EndDate)
                {
                    return ResponseUtil.Error(ResponseMessages.TaskStartDateBeforeLastTaskEndDate,
                        ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                }

                // Kiểm tra EndDate của task mới với StartDate của task đầu tiên của Step tiếp theo
                
            }


            //////////////////////////////////////////////////////


            var lastStepInCurrentFlow = orderedStepsInFlow.LastOrDefault();
            if (lastStepInCurrentFlow != null && lastStepInCurrentFlow.StepId == taskDto.StepId)
            {
                // Lấy tất cả task trong Step cuối cùng của Flow hiện tại
                var tasksInLastStep =
                    await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(lastStepInCurrentFlow.StepId,
                        taskDto.DocumentId);
                var orderedTasksInLastStep = tasksInLastStep.OrderBy(t => t.TaskNumber).ToList();

                // Lấy task cuối cùng trong Step cuối cùng của Flow hiện tại
                var lastTaskInCurrentFlow = orderedTasksInLastStep.LastOrDefault();

                // Kiểm tra giữa các Flow trong Workflow, chỉ khi bước hiện tại là Step cuối trong Flow
                var allFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
                var orderedFlows = allFlows.OrderBy(f => f.FlowNumber).ToList();

                // Lấy Flow tiếp theo nếu có
                var nextWorkflowFlowFlow = orderedFlows.Where(f => f.FlowNumber == workflowFlow.FlowNumber + 1)
                    .FirstOrDefault();
                if (nextWorkflowFlowFlow != null)
                {
                    // Lấy Step đầu tiên của Flow tiếp theo
                    var nextFlow = await _unitOfWork.FlowUOW.FindFlowByIdAsync(nextWorkflowFlowFlow.FlowId);
                    var stepsInNextFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(currentFlow);
                    var orderedStepsNextInFlow = stepsInNextFlow.OrderBy(s => s.StepNumber).ToList();
                    var firstStepInNextFlow = orderedStepsNextInFlow.FirstOrDefault(s => s.FlowId == nextFlow.FlowId);
                    if (firstStepInNextFlow != null)
                    {
                        // Lấy tất cả Task trong Step đầu tiên của Flow tiếp theo
                        var firstTaskInNextFlow =
                            await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(firstStepInNextFlow.StepId,
                                taskDto.DocumentId);
                        var firstTaskInNextFlowOrdered =
                            firstTaskInNextFlow.OrderBy(t => t.TaskNumber).FirstOrDefault();

                        // Kiểm tra EndDate của task mới với StartDate của task đầu tiên trong Step đầu tiên của Flow tiếp theo
                        if (firstTaskInNextFlowOrdered != null)
                        {
                            if (taskDto.EndDate >= firstTaskInNextFlowOrdered.StartDate)
                            {
                                return ResponseUtil.Error(ResponseMessages.TaskEndDateAfterNextFlowStartDate,
                                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                            }
                        }
                    }
                }
            }


            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(taskDto.UserId.Value);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var userRoles = user.UserRoles
                .Select(ur => ur.Role.RoleName)
                .ToList();
            var primaryRoles = userRoles
                .Select(name =>
                {
                    // Nếu role có chứa "_", lấy phần cuối sau dấu "_"
                    if (name.Contains("_"))
                    {
                        var parts = name.Split('_');
                        return parts[^1].Trim().ToLower(); // phần cuối cùng
                    }
                    return name.Trim().ToLower();
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var matchedRoles = primaryRoles.Where(role => role.Equals(currentStep.Role.RoleName.ToLower())).ToList();
            if (!matchedRoles.Any())
            {
                return ResponseUtil.Error(ResponseMessages.UserNotRoleWithStep, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            
            if (taskDto.TaskType == TaskType.Sign)
            {
                if (!(currentStep.Role.RoleName.ToLower().Equals("leader") ||
                    currentStep.Role.RoleName.ToLower().Equals("chief")))
                {
                   return ResponseUtil.Error(
                        ResponseMessages.UserNotRoleWithTaskTypeIsSign,
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
                
                var userTasksInDocument = await _unitOfWork.TaskUOW.FindTaskByDocumentIdAndUserIdAsync(taskDto.DocumentId.Value,
                    taskDto.UserId.Value);
                var existingSignTask = userTasksInDocument
                    .FirstOrDefault(t => t.TaskType == TaskType.Sign && t.IsDeleted == false);
                if (existingSignTask != null)
                {
                    return ResponseUtil.Error(
                        "Người dùng chỉ được phép có duy nhất một nhiệm vụ ký trong tài liệu.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }

            

            if (document.Deadline < taskDto.EndDate)
            {
                return ResponseUtil.Error(ResponseMessages.TaskEndDateGreaterDeadline,
                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            var orderedTasks = await GetOrderedTasks(document.Tasks.Where(t => t.IsDeleted == false).ToList(),
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            if (orderedTasks.Count == 0)
                return ResponseUtil.Error(ResponseMessages.TaskFirstNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            //if(orderedTasks[orderedTasks.Count - 1].EndDate > taskDto.StartDate)
            //    return ResponseUtil.Error(ResponseMessages.TaskStartdayLowerEndDaypreviousStepFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
            
            //////////////////////////////
            
            
            //var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(taskDto.DocumentId.Value);
            var scope = document.DocumentWorkflowStatuses.FirstOrDefault()?.Workflow.Scope;
            if (scope != Scope.InComing)
            {
                // Lấy tất cả task trong tài liệu để xác định vị trí task mới

                if (scope == Scope.Division)
                {
                    var userOfFirstTask = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTasks[0].UserId);
                    if (userOfFirstTask == null)
                    {
                        return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }
                    if (userOfFirstTask.DivisionId != user.DivisionId)
                    {
                        return ResponseUtil.Error(
                            "Người dùng không nằm trong cùng một phoòng ban.",
                            ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);
                    }
                }

            var totalTasksInDocument = orderedTasks.Count();
                var newTaskPosition = totalTasksInDocument + 1;

                // Kiểm tra task thứ 2
                if (newTaskPosition == 2)
                {
                    if (taskDto.TaskType != TaskType.Upload)
                    {
                        return ResponseUtil.Error(
                            "Nhiệm vụ thứ 2 trong luồng phải có loại nhiệm vụ là Tải văn bản lên .",
                            ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);
                    }
                }
                // Kiểm tra task không phải thứ 2
                else if (taskDto.TaskType == TaskType.Create || taskDto.TaskType == TaskType.Upload)
                {
                    return ResponseUtil.Error(
                        "Không thể tạo thêm nhiệm vụ với nhiệm vụ chính là Tạo văn bản hoặc Tải văn bản lên trong luồng.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            else
            {
                if (taskDto.TaskType == TaskType.Upload)
                {
                    return ResponseUtil.Error(
                        "Nhiệm vụ không được có loại nhiệm vụ Tải văn bản lên trong luồng đến.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            
            
            // Flatten tất cả các Step trong tất cả các Flow, kèm theo FlowNumber để dễ so sánh
            var allStepsWithFlowNumber = workflowFlowAll
                .SelectMany(wf => wf.Flow.Steps.Select(s => new
                {
                    Step = s,
                    FlowNumber = wf.FlowNumber,
                    StepNumber = s.StepNumber
                }))
                .ToList();
            
            

             // Lọc ra các Step nằm "trước" currentStep theo thứ tự FlowNumber và StepNumber
            var previousSteps = allStepsWithFlowNumber
                .Where(x => 
                    (x.FlowNumber < currentFlowNumber) ||
                    (x.FlowNumber == currentFlowNumber && x.StepNumber < currentStep.StepNumber))
                .OrderByDescending(x => x.FlowNumber)
                .ThenByDescending(x => x.StepNumber)
                .ToList();

            var previousStep = previousSteps.FirstOrDefault()?.Step;
            if (previousStep != null)
            {
                var taskInPreviousStep =
                    await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(previousStep.StepId, taskDto.DocumentId);
                if (taskInPreviousStep.Count() == 0)
                {
                    return ResponseUtil.Error(ResponseMessages.TaskInPreviousStepNotFound,
                        ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                }
            }
            
            if (orderedTasks[0].TaskStatus == TasksStatus.Completed)
            {
                var taskOfUser = orderedTasks.Where(t => t.UserId == userId && t.TaskType == TaskType.Create).ToList();
                if (taskOfUser.Any())
                {
                }
                else
                {
                    return ResponseUtil.Error(ResponseMessages.TaskCanNotCreate, ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }

            // neu task của người tạo là create và đã complete thì kkhoogn được tạo task nua
            var createTasks = orderedTasks.Where(t => t.TaskType == TaskType.Create).ToList();
            if (createTasks.Any() && taskDto.TaskType == TaskType.Create)
            {
                return ResponseUtil.Error(
                    ResponseMessages.OnlyOneCreateTaskAllowed,
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            if (taskDto.StepId == firstStepInFlow!.StepId && orderedTasks.Count == 0)
            {
                taskDto.TaskStatus = TasksStatus.InProgress;
                // if (taskDto.DocumentId == null)
                // {
                //     return ResponseUtil.Error(ResponseMessages.DocumentIdNull, ResponseMessages.OperationFailed,
                //         HttpStatusCode.BadRequest);
                // }
            }
            else
            {
                taskDto.TaskStatus = TasksStatus.Waiting;
            }

            taskDto.TaskNumber = nextTaskNumber;
            taskDto.CreatedDate = DateTime.Now;
            taskDto.IsDeleted = false;
            taskDto.IsActive = true;
            var userCreate = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            taskDto.TaskId = Guid.NewGuid();
            var task = new Tasks
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                StartDate = taskDto.StartDate,
                EndDate = taskDto.EndDate,
                TaskStatus = taskDto.TaskStatus ?? TasksStatus.Waiting,
                TaskType = taskDto.TaskType,
                CreatedDate = DateTime.Now,
                TaskNumber = taskDto.TaskNumber,
                IsDeleted = taskDto.IsDeleted ?? false,
                IsActive = taskDto.IsActive ?? true,
                StepId = taskDto.StepId ?? Guid.Empty,
                DocumentId = taskDto.DocumentId ?? null,
                UserId = taskDto.UserId ?? Guid.Empty,
                CreatedBy = userCreate!.FullName,
            };
            await _unitOfWork.TaskUOW.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(taskDto, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<bool> IsSubmitAllowedInFirstTaskOfNonFirstFlow(
        Guid currentFlowId,
        int currentStepNumber,
        int currentTaskNumber,
        TaskType taskTypeBeingCreated,
        Guid currentUserOfTask)
    {
        // Nếu không phải Task đầu tiên của Step đầu tiên thì bỏ qua điều kiện này
        if (currentStepNumber != 1 || currentTaskNumber != 1)
            return true;

        // Nếu không phải task Submit thì không cần kiểm tra
        if (taskTypeBeingCreated != TaskType.Submit)
            return true;

        // Lấy thông tin Flow hiện tại và toàn bộ Flow trong Workflow
        var currentWorkflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByFlowIdAsync(currentFlowId);
        var allFlowsInWorkflow =
            await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(currentWorkflowFlow.WorkflowId);

        var orderedFlows = allFlowsInWorkflow.OrderBy(f => f.FlowNumber).ToList();
        var currentFlowIndex = orderedFlows.FindIndex(f => f.FlowId == currentFlowId);

        // Nếu là Flow đầu tiên thì không áp dụng điều kiện này → cho phép tạo Submit
        if (currentFlowIndex == 0)
            return true;

        // Lấy Flow trước đó
        var previousFlowId = orderedFlows[currentFlowIndex - 1].FlowId;
        var stepsOfPreviousFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(previousFlowId);
        var lastStep = stepsOfPreviousFlow.OrderByDescending(s => s.StepNumber).FirstOrDefault();

        if (lastStep != null)
        {
            var tasksInLastStep = await _unitOfWork.TaskUOW.FindTaskByStepIdAsync(lastStep.StepId);
            var lastTask = tasksInLastStep.OrderByDescending(t => t.TaskNumber).FirstOrDefault();

            if (lastTask?.TaskType == TaskType.Submit)
            {
                return false; // Không cho phép Submit ở đầu Flow này
            }

            if (lastTask?.UserId != currentUserOfTask)
            {
                return false;
            }
        }

        return true; //Được phép tạo Task Submit
    }


    public async Task<ResponseDto> CreateFirstTask(TaskDto taskDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskDto.Title) || string.IsNullOrWhiteSpace(taskDto.Description))
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.UserId == null)
                return ResponseUtil.Error(ResponseMessages.UserIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskDto.DocumentId == null)
                return ResponseUtil.Error(ResponseMessages.DocumentIdNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);


            if (taskDto.StartDate < DateTime.Now || taskDto.EndDate < DateTime.Now)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (taskDto.EndDate <= taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(taskDto.DocumentId.Value);
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            var workflowId = document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty;
            var workflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var firstFlowInWorkflow = workflowFlow!.OrderBy(wf => wf.FlowNumber).FirstOrDefault();
            var stepAllOfFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(firstFlowInWorkflow!.FlowId);
            var firstStepInFlow = stepAllOfFlow!.OrderBy(s => s.StepNumber).FirstOrDefault();

            var existingTasks =
                await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(firstStepInFlow.StepId, taskDto.DocumentId);
            var nextTaskNumber = existingTasks.Count() + 1;

            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(taskDto.UserId.Value);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var userRoles = user.UserRoles
                .Select(ur => ur.Role.RoleName)
                .ToList();
            var primaryRoles = userRoles
                .Select(name =>
                {
                    // Nếu role có chứa "_", lấy phần cuối sau dấu "_"
                    if (name.Contains("_"))
                    {
                        var parts = name.Split('_');
                        return parts[^1].Trim().ToLower(); // phần cuối cùng
                    }

                    return name.Trim().ToLower();
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var stepInFlowFirst = await _unitOfWork.StepUOW.FindStepByIdAsync(firstStepInFlow.StepId);

            var matchedRoles = primaryRoles.Where(role => role.Equals(stepInFlowFirst.Role.RoleName.ToLower()))
                .ToList();
            if (!matchedRoles.Any())
            {
                return ResponseUtil.Error(ResponseMessages.UserNotRoleWithStep, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            taskDto.TaskStatus = TasksStatus.InProgress;
            taskDto.TaskNumber = nextTaskNumber;
            taskDto.CreatedDate = DateTime.Now;
            taskDto.IsDeleted = false;
            taskDto.IsActive = true;
            var userCreate = await _unitOfWork.UserUOW.FindUserByIdAsync(taskDto.UserId.Value);
            taskDto.TaskId = Guid.NewGuid();
            var task = new Tasks
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                StartDate = taskDto.StartDate,
                EndDate = taskDto.EndDate,
                TaskStatus = taskDto.TaskStatus ?? TasksStatus.Waiting,
                TaskType = taskDto.TaskType,
                CreatedDate = DateTime.Now,
                TaskNumber = taskDto.TaskNumber,
                IsDeleted = taskDto.IsDeleted ?? false,
                IsActive = taskDto.IsActive ?? true,
                StepId = firstStepInFlow.StepId,
                DocumentId = taskDto.DocumentId ?? null,
                UserId = taskDto.UserId ?? Guid.Empty,
                CreatedBy = userCreate!.FullName,
            };
            await _unitOfWork.TaskUOW.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(taskDto, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> DeleteTaskAsync(Guid id)
    {
        try
        {
            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(id);
            if (task == null)
                return ResponseUtil.Error(ResponseMessages.TaskNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (task.IsDeleted)
            {
                return ResponseUtil.Error(ResponseMessages.TaskAlreadyDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            
            var workflowId = task.Document!.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId;
            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId ?? Guid.Empty);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(task.DocumentId!.Value);
            var orderedTasks = await GetOrderedTasks(document.Tasks, workflowId ?? Guid.Empty);
            if (workflow.Scope == Scope.InComing && task.TaskStatus == TasksStatus.Waiting)
            {
                
            }
            else
            {
                if (orderedTasks[0].TaskStatus == TasksStatus.Completed)
                    return ResponseUtil.Error(ResponseMessages.TaskCanNotDelete, ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                if (orderedTasks[0].TaskId == id)
                {
                    return ResponseUtil.Error(ResponseMessages.TaskFirstCanNotDelete, ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }

            

            var stepId = task.StepId;
            var deletedTaskNumber = task.TaskNumber;

            task.IsDeleted = true;
            await _unitOfWork.TaskUOW.UpdateAsync(task);

            var tasksToUpdate = await _unitOfWork.TaskUOW.FindTaskByStepIdAsync(stepId);
            var affectedTasks = tasksToUpdate
                .Where(t => t.TaskNumber > deletedTaskNumber)
                .OrderBy(t => t.TaskNumber)
                .ToList();

            foreach (var t in affectedTasks)
            {
                t.TaskNumber -= 1;
                await _unitOfWork.TaskUOW.UpdateAsync(t);
            }

            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(task, ResponseMessages.DeleteSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> UpdateTaskAsync(TaskRequest taskRequest)
    {
        try
        {
            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(taskRequest.TaskId);
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(taskRequest.UserId.Value);
            if (task == null)
                return ResponseUtil.Error(ResponseMessages.TaskNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (task.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.TaskAlreadyDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (task.TaskStatus != TasksStatus.Waiting && task.TaskStatus != TasksStatus.Revised)
                return ResponseUtil.Error(ResponseMessages.TaskCanNotUpdate, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            if (taskRequest.StartDate < DateTime.Now || taskRequest.EndDate < DateTime.Now)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskRequest.EndDate <= taskRequest.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(task.DocumentId);
            var orderedTasks = await GetOrderedTasks(document.Tasks.Where(t => t.IsDeleted == false).ToList(),
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            // if(orderedTasks[orderedTasks.Count - 1].EndDate > taskRequest.StartDate)
            //     return ResponseUtil.Error(ResponseMessages.TaskStartdayLowerEndDaypreviousStepFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            var currentIndex = orderedTasks.FindIndex(t => t.TaskId == taskRequest.TaskId);

            if (currentIndex > 0)
            {
                var previousTask = orderedTasks[currentIndex - 1];
                if (previousTask.EndDate > taskRequest.StartDate)
                {
                    return ResponseUtil.Error(
                        ResponseMessages.TaskStartdayLowerEndDaypreviousStepFailed,
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest
                    );
                }
            }
            
            // var nexTask = orderedTasks[currentIndex + 1];
            // if (nexTask.StartDate < taskRequest.EndDate)
            // {
            //     return ResponseUtil.Error(
            //         ResponseMessages.TaskStartdayNextTaskLowerEndDayCurrentTask,
            //         ResponseMessages.OperationFailed,
            //         HttpStatusCode.BadRequest
            //     );
            // }
            
            if (currentIndex < orderedTasks.Count - 1) // Kiểm tra xem có task tiếp theo hay không
            {
                var nextTask = orderedTasks[currentIndex + 1];
                if (nextTask.StartDate < taskRequest.EndDate && nextTask.StartDate != DateTime.MinValue)
                {
                    return ResponseUtil.Error(
                        ResponseMessages.TaskStartdayNextTaskLowerEndDayCurrentTask,
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest
                    );
                }
            }

            if (orderedTasks[0].TaskId == taskRequest.TaskId)
            {
                return ResponseUtil.Error(ResponseMessages.TaskFirstCanNotUpdate, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            var scope = document.DocumentWorkflowStatuses.FirstOrDefault()?.Workflow.Scope;
            if (scope == Scope.Division)
            {
                var userOfFirstTask = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTasks[0].UserId);
                if (userOfFirstTask == null)
                {
                    return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                }
                if (userOfFirstTask.DivisionId != user.DivisionId)
                {
                    return ResponseUtil.Error(
                        "Người dùng không nằm trong cùng một phòng ban.",
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
            }
            
            
            
            var tasksInCurrentStep = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(task.StepId, document.DocumentId);
        var otherTasksInStep = tasksInCurrentStep.Where(t => t.TaskId != taskRequest.TaskId).ToList();
        if (otherTasksInStep.Any())
        {
            var existingUserIds = otherTasksInStep.Select(t => t.UserId).Distinct().ToList();
            if (existingUserIds.Any() && !existingUserIds.Contains(taskRequest.UserId.Value))
            {
                return ResponseUtil.Error(
                    "Step này đã có task của người dùng khác. Chỉ một người dùng được phép có task trong cùng step.",
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
        }
        // *** KẾT THÚC: Kiểm tra chỉ một người dùng trong step ***

        // *** THÊM: Kiểm tra nếu task là task đầu tiên của step đầu tiên trong flow không phải đầu tiên ***
        var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(task.StepId);
        var currentFlow = currentStep.FlowId;
        var stepsInFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(currentFlow);
        var orderedStepsInFlow = stepsInFlow.OrderBy(s => s.StepNumber).ToList();
        var currentStepIndex = orderedStepsInFlow.FindIndex(s => s.StepId == task.StepId);

        // Kiểm tra nếu task là task đầu tiên của step đầu tiên
        if (!otherTasksInStep.Any() && currentStepIndex == 0) // Task đầu tiên của step đầu tiên
        {
            var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            var currentFlowNumber = workflowFlowAll
                .FirstOrDefault(wf => wf.FlowId == currentFlow)?.FlowNumber ?? 0;

            if (currentFlowNumber > 1) // Có flow trước đó
            {
                var previousFlow = workflowFlowAll
                    .Where(wf => wf.FlowNumber == currentFlowNumber - 1)
                    .FirstOrDefault();

                if (previousFlow != null)
                {
                    var stepsInPreviousFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(previousFlow.FlowId);
                    var lastStepInPreviousFlow = stepsInPreviousFlow.OrderBy(s => s.StepNumber).LastOrDefault();

                    if (lastStepInPreviousFlow != null)
                    {
                        var tasksInLastStep = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(
                            lastStepInPreviousFlow.StepId, document.DocumentId);
                        var lastTaskInPreviousFlow = tasksInLastStep.OrderBy(t => t.TaskNumber).LastOrDefault();

                        if (lastTaskInPreviousFlow != null)
                        {
                            // Kiểm tra nếu task cuối của flow trước đó là Submit
                            if (lastTaskInPreviousFlow.TaskType == TaskType.Submit)
                            {
                                return ResponseUtil.Error(
                                    "Không thể cập nhật task vì task cuối cùng của flow trước đó là Submit.",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }

                            // Kiểm tra UserId nếu không phải Submit
                            if (lastTaskInPreviousFlow.UserId != taskRequest.UserId.Value)
                            {
                                return ResponseUtil.Error(
                                    "UserId của task cập nhật phải khớp với UserId của task cuối cùng trong step cuối cùng của flow trước đó.",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }
                        }
                    }
                }
            }
        }
        // *** KẾT THÚC: Kiểm tra task đầu tiên của step đầu tiên ***

        // Kiểm tra deadline của document
        if (document.Deadline < taskRequest.EndDate)
        {
            return ResponseUtil.Error(ResponseMessages.TaskEndDateGreaterDeadline,
                ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
        }
            
            
        if (task.TaskStatus == TasksStatus.Revised)
        {
            if (task.StartDate == DateTime.MinValue)
            {
                task.TaskStatus = TasksStatus.Waiting;
            }
        }

            var hasChanges = false;

            if (!string.IsNullOrWhiteSpace(taskRequest.Title) && !task.Title.Equals(taskRequest.Title))
            {
                task.Title = taskRequest.Title;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(taskRequest.Description) &&
                !task.Description.Equals(taskRequest.Description))
            {
                task.Description = taskRequest.Description;
                hasChanges = true;
            }

            if (taskRequest.StartDate != null && task.StartDate != taskRequest.StartDate)
            {
                task.StartDate = taskRequest.StartDate.Value;
                hasChanges = true;
            }

            if (taskRequest.EndDate != null && task.EndDate != taskRequest.EndDate)
            {
                task.EndDate = taskRequest.EndDate.Value;
                hasChanges = true;
            }



            if (taskRequest.UserId != null && task.UserId != taskRequest.UserId)
            {
                if (user == null)
                    return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                var userRoles = user.UserRoles
                    .Select(ur => ur.Role.RoleName)
                    .ToList();
                var primaryRoles = userRoles
                    .Select(name =>
                    {
                        // Nếu role có chứa "_", lấy phần cuối sau dấu "_"
                        if (name.Contains("_"))
                        {
                            var parts = name.Split('_');
                            return parts[^1].Trim().ToLower(); // phần cuối cùng
                        }

                        return name.Trim().ToLower();
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                //var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(task.StepId);
                var matchedRoles = primaryRoles.Where(role => role.Equals(currentStep.Role.RoleName.ToLower()))
                    .ToList();
                if (!matchedRoles.Any())
                {
                    return ResponseUtil.Error(ResponseMessages.UserNotRoleWithStep, ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }
                else
                {
                    task.UserId = taskRequest.UserId.Value;
                    hasChanges = true;
                }
            }

            if (!hasChanges)
                return ResponseUtil.GetObject(ResponseMessages.NoChangesDetected, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 0);
            await _unitOfWork.TaskUOW.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
            var notification = _notificationService.CreateTaskAssignNotification(task, task.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _notificationService.SendPushNotificationMobileAsync(user.FcmToken, notification);
            await _hubContext.Clients.User(notification.UserId.ToString())
                .SendAsync("ReceiveMessage", notification);
            return ResponseUtil.GetObject(ResponseMessages.TaskHasUpdatedInformation,
                ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> FindAllTasksAsync(Guid userId, int page, int limit)
    {
        try
        {
            var tasks = await _unitOfWork.TaskUOW.FindAllTaskAsync(userId);

            var taskDetails = new List<TaskDetail>();
            tasks = tasks.OrderByDescending(t => t.CreatedDate);

            foreach (var task in tasks)
            {
                var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(task.DocumentId!.Value);
                if (document == null)
                    return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                var orderedTasks = await GetOrderedTasks(document.Tasks,
                    document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
                
                
                
                var taskDto = _mapper.Map<TaskDto>(task);
                var taskDetail = new TaskDetail();
                
                if(task.TaskType == TaskType.Sign)
                {
                    var listDigitalCertificates =
                        await _unitOfWork.DigitalCertificateUOW.FindDigitalCertificateByUserIdAsync(userId);
                    var digitalCertificate = listDigitalCertificates.Where(dc => dc.IsUsb != null).FirstOrDefault();
                    taskDetail.IsUsb = digitalCertificate?.IsUsb;
                }
                else
                {
                    taskDetail.IsUsb = null;
                }
                
                taskDetail.TaskDto = taskDto;
                taskDetail.Scope = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope;
                taskDetail.WorkflowId = task.Document.DocumentWorkflowStatuses.FirstOrDefault().WorkflowId;
                taskDetail.WorkflowName = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowName;
                taskDetail.StepAction = task.Step.Action;
                taskDetail.DocumentId = document.DocumentId;
                taskDetail.DocumentName = document.DocumentName;
                taskDetail.DocumentTypeName = task.Document.DocumentType.DocumentTypeName;
                var user = await _unitOfWork.UserUOW.FindUserByIdAsync(task.UserId);
                taskDetail.UserDoTask = user?.FullName;
                taskDetail.UserNameCreateTask = task.CreatedBy;
                taskDetails.Add(taskDetail);
            }


            var totalRecords = tasks.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / limit);


            IEnumerable<TaskDetail> tasksResults = taskDetails
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            // var result = _mapper.Map<IEnumerable<TaskDto>>(tasksResults);
            return ResponseUtil.GetCollection(tasksResults, ResponseMessages.GetSuccessfully, HttpStatusCode.OK,
                tasks.Count(), 1, 10, totalPages);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> FindTaskByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.TaskIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(id);
            if (task == null)
                return ResponseUtil.Error(ResponseMessages.TaskNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(task.DocumentId!.Value);
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var orderedTasks = await GetOrderedTasks(document.Tasks,
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);

            var taskDto = _mapper.Map<TaskDto>(task);
            var verion = "0";
            if (task.TaskStatus is TasksStatus.Completed or TasksStatus.InProgress)
                verion = task.Document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion).VersionNumber;
            var result = new TaskDetail();
            
            if(task.TaskType == TaskType.Sign)
            {
                var listDigitalCertificates =
                    await _unitOfWork.DigitalCertificateUOW.FindDigitalCertificateByUserIdAsync(task.UserId);
                var digitalCertificate = listDigitalCertificates.Where(dc => dc.IsUsb != null).FirstOrDefault();
                result.IsUsb = digitalCertificate?.IsUsb;
            }
            else
            {
                result.IsUsb = null;
            }
            
            result.TaskDto = taskDto;
            result.Scope = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope;
            result.WorkflowName = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowName;
            result.WorkflowId = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowId;
            result.StepAction = task.Step.Action;
            result.DocumentId = document.DocumentId;
            result.DocumentName = document.DocumentName;
            result.DocumentTypeName = task.Document.DocumentType.DocumentTypeName;
            result.FileSize = _fileService.GetFileSize(task.Document.DocumentId,
                task.Document.DocumentVersions.FirstOrDefault(x => x.VersionNumber == verion).DocumentVersionId,
                task.Document.DocumentName);

            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(task.UserId);
            result.UserDoTask = user?.FullName;
            result.UserNameCreateTask = task.CreatedBy;
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> FindAllTaskByDocumentIdAsync(Guid documentId)
    {
        try
        {
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var orderedTasks = await GetOrderedTasks(
                document.Tasks,
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty
            );
            var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(orderedTasks);
            return ResponseUtil.GetObject(taskDtos, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ResponseDto> GetDocumentsByTabForUser(String? docName, Scope? scope, Guid userId, DocumentTab tab,
        int page, int limit)
    {
        try
        {
            var allDocuments = await _unitOfWork.DocumentUOW.FindAllDocumentForTaskAsync(userId);

            var now = DateTime.UtcNow;

            IEnumerable<Document> filteredDocuments = new List<Document>();
            int totalRecords = 0;
            int totalPages = 0;
            IEnumerable<Document> documentResults = new List<Document>();
            IEnumerable<DocumentRejectResponse> documentRejectResults = new List<DocumentRejectResponse>();
            IEnumerable<DocumentDto> result = new List<DocumentDto>();

            switch (tab)
            {
                case DocumentTab.All:
                    filteredDocuments = allDocuments
                        .Where(doc => doc.Tasks != null && doc.Tasks
                            .Any(task => task.UserId == userId && task.TaskStatus != TasksStatus.Waiting))
                        .ToList();
                    break;

                case DocumentTab.Overdue:
                {
                    filteredDocuments = allDocuments
                        .Where(d => d.Tasks.Any(t => t.UserId == userId &&
                                                     t.TaskStatus == TasksStatus.Waiting &&
                                                     d.Deadline < now))
                        .ToList();
                    break;
                }

                case DocumentTab.Rejected:
                {
                    List<DocumentRejectResponse> documentRejectResponses = new List<DocumentRejectResponse>();
                    foreach (var allDocument in allDocuments)
                    {
                        var documentVersions =
                            await _unitOfWork.DocumentVersionUOW.FindDocumentVersionByDocumentIdAsync(allDocument
                                .DocumentId);

                        var rejectedVersions = documentVersions
                            .Where(v => v.IsFinalVersion == false && !v.VersionNumber.Equals("0"))
                            .ToList();
                         // var rejectedVersionsIncoming = documentVersions
                         //     .Where(v => v.IsFinalVersion == false && v.VersionNumber.Equals("0"))
                         //     .ToList();
                        

                        if (!rejectedVersions.Any())
                            continue;
                         // if (!rejectedVersionsIncoming.Any())
                         //     continue;

                        List<VersionOfDocResponse> versionOfDocResponses = new List<VersionOfDocResponse>();
                        foreach (var documentVersion in rejectedVersions)
                        {
                            var rejectComment = documentVersion.Comments.FirstOrDefault();
                            if (rejectComment == null) continue;
                            var documentVersionRes = new VersionOfDocResponse
                            {
                                VersionId = documentVersion.DocumentVersionId,
                                VersionNumber = documentVersion.VersionNumber,
                                DateReject = rejectComment.CreateDate,
                                UserIdReject = rejectComment.UserId,
                                UserReject = rejectComment.User.UserName
                            };
                            versionOfDocResponses.Add(documentVersionRes);
                        }
                        
                        // foreach (var documentVersion in rejectedVersionsIncoming)
                        // {
                        //     var rejectComment = documentVersion.Comments.FirstOrDefault();
                        //     if (rejectComment == null) continue;
                        //     var documentVersionRes = new VersionOfDocResponse
                        //     {
                        //         VersionId = documentVersion.DocumentVersionId,
                        //         VersionNumber = documentVersion.VersionNumber,
                        //         DateReject = rejectComment.CreateDate,
                        //         UserIdReject = rejectComment.UserId,
                        //         UserReject = rejectComment.User.UserName
                        //     };
                        //     versionOfDocResponses.Add(documentVersionRes);
                        // }

                        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(allDocument.UserId);
                        if (user == null)
                            return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                                HttpStatusCode.NotFound);
                        var documentRejectResponse = new DocumentRejectResponse
                        {
                            DocumentId = allDocument.DocumentId,
                            DocumentName = allDocument.DocumentName,
                            FullName = user.FullName,
                            WorkflowName = allDocument.DocumentWorkflowStatuses.FirstOrDefault()?.Workflow.WorkflowName,
                            Scope = allDocument.DocumentWorkflowStatuses.FirstOrDefault()?.Workflow.Scope,
                            DocumentType = allDocument.DocumentType.DocumentTypeName,
                            VersionOfDocResponses = versionOfDocResponses
                        };
                        documentRejectResponses.Add(documentRejectResponse);
                    }

                    if (!string.IsNullOrWhiteSpace(docName))
                    {
                        documentRejectResponses = documentRejectResponses
                            .Where(d => d.DocumentName != null && d.DocumentName.ToLower()
                                .Contains(docName.ToLower(), StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    if (scope.HasValue)
                    {
                        documentRejectResponses = documentRejectResponses
                            .Where(d => allDocuments.FirstOrDefault(doc => doc.DocumentId == d.DocumentId)
                                ?.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope == scope.Value)
                            .ToList();
                    }

                    totalRecords = documentRejectResponses.Count();
                    totalPages = (int)Math.Ceiling((double)totalRecords / limit);
                    documentRejectResults = documentRejectResponses
                        .Skip((page - 1) * limit)
                        .Take(limit)
                        .ToList();


                    return ResponseUtil.GetCollection(documentRejectResults, ResponseMessages.GetSuccessfully,
                        HttpStatusCode.OK, totalRecords, page, limit, totalPages);

                    // docId, docName, docType, versionId, versionNumber, date reject, user reject
                }


                case DocumentTab.Accepted:
                {
                    var acceptedDocuments = new List<Document>();

                    foreach (var doc in allDocuments)
                    {
                        var orderedTasks = await GetOrderedTasks(
                            doc.Tasks.Where(t => t.IsDeleted == false).ToList(),
                            doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty
                        );

                        // for (int i = 0; i < orderedTasks.Count - 1; i++)
                        // {
                        //     var currentTask = orderedTasks[i];
                        //     var nextTask = orderedTasks[i + 1];
                        //
                        //     if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                        //     {
                        //         if (nextTask.TaskStatus == TasksStatus.Completed && nextTask.UserId != userId)
                        //         {
                        //             acceptedDocuments.Add(doc);
                        //             break;
                        //         }
                        //     }
                        // }
                        
                        // *** THÊM: Ưu tiên Waiting - Bỏ qua tài liệu nếu thỏa mãn điều kiện Waiting ***
                        bool isWaiting = false;
                        for (int j = 0; j < orderedTasks.Count - 1; j++)
                        {
                            var currentTask = orderedTasks[j];
                            var nextTask = orderedTasks[j + 1];
                            if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                            {
                                if (nextTask.TaskStatus == TasksStatus.InProgress && nextTask.UserId != userId)
                                {
                                    isWaiting = true;
                                    break;
                                }
                            }
                        }
                        if (isWaiting)
                        {
                            continue; // Bỏ qua tài liệu vì nó thuộc Waiting
                        }
                        // *** KẾT THÚC: Kiểm tra ưu tiên Waiting ***

                        // Logic gốc: Kiểm tra task cho Accepted
                        for (int i = 0; i < orderedTasks.Count - 1; i++)
                        {
                            var currentTask = orderedTasks[i];
                            var nextTask = orderedTasks[i + 1];

                            if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                            {
                                if (nextTask.TaskStatus == TasksStatus.Completed && nextTask.UserId != userId)
                                {
                                    acceptedDocuments.Add(doc);
                                    break;
                                }
                            }
                        }

                        var lastTask = orderedTasks.LastOrDefault();
                        if (lastTask != null &&
                            lastTask.UserId == userId &&
                            lastTask.TaskStatus == TasksStatus.Completed &&
                            !acceptedDocuments.Contains(doc)) // tránh thêm trùng nếu đã thêm ở trên
                        {
                            acceptedDocuments.Add(doc);
                        }
                    }

                    filteredDocuments = acceptedDocuments;
                    break;
                }


                case DocumentTab.PendingApproval:
                {
                    var pendingApprovalDocuments = new List<Document>();

                    foreach (var doc in allDocuments)
                    {
                        var orderedTasks = await GetOrderedTasks(
                            doc.Tasks.Where(t => t.IsDeleted == false).ToList(),
                            doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty
                        );

                        for (int i = 0; i < orderedTasks.Count; i++)
                        {
                            var currentTask = orderedTasks[i];

                            if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.InProgress /*&&
                                (currentTask.TaskType == TaskType.Browse || currentTask.TaskType == TaskType.View || currentTask.TaskType == TaskType.Upload)*/)
                            {
                                bool previousTasksCompleted = orderedTasks
                                    .Take(i)
                                    .All(t => t.TaskStatus == TasksStatus.Completed);

                                if (previousTasksCompleted)
                                {
                                    pendingApprovalDocuments.Add(doc);
                                    break;
                                }
                            }
                        }
                    }

                    filteredDocuments = pendingApprovalDocuments;
                    break;
                }

                case DocumentTab.Waiting:
                {
                    var waitingDocuments = new List<Document>();

                    foreach (var doc in allDocuments)
                    {
                        var orderedTasks = await GetOrderedTasks(doc.Tasks.Where(t => t.IsDeleted == false).ToList(),
                            doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);

                        // for (int i = 0; i < orderedTasks.Count - 1; i++)
                        // {
                        //     var currentTask = orderedTasks[i];
                        //     var nextTask = orderedTasks[i + 1];
                        //
                        //     if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                        //     {
                        //         if (nextTask.TaskStatus == TasksStatus.InProgress && nextTask.UserId != userId)
                        //         {
                        //             waitingDocuments.Add(doc);
                        //             break; // Thỏa điều kiện => thêm document và thoát vòng lặp
                        //         }
                        //
                        //         if (nextTask.UserId != userId && currentTask.TaskType != TaskType.Browse)
                        //         {
                        //             waitingDocuments.Add(doc);
                        //             break;
                        //         }
                        //     }
                        // }
                        for (int i = 0; i < orderedTasks.Count - 1; i++)
                        {
                            var currentTask = orderedTasks[i];
                            var nextTask = orderedTasks[i + 1];

                            if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                            {
                                if (nextTask.TaskStatus == TasksStatus.InProgress && nextTask.UserId != userId)
                                {
                                    waitingDocuments.Add(doc);
                                    break;
                                }
                            }
                        }
                    }

                    filteredDocuments = waitingDocuments;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(docName))
            {
                filteredDocuments = filteredDocuments
                    .Where(d => !string.IsNullOrEmpty(d.DocumentName) &&
                                d.DocumentName.Contains(docName, StringComparison.OrdinalIgnoreCase));
            }

            if (scope.HasValue)
            {
                filteredDocuments = filteredDocuments
                    .Where(d => d.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope == scope.Value);
            }

            totalRecords = filteredDocuments.Count();
            totalPages = (int)Math.Ceiling((double)totalRecords / limit);
            documentResults = filteredDocuments
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            result = _mapper.Map<IEnumerable<DocumentDto>>(documentResults);

            List<DocumentTabResponse> documentResponses = new List<DocumentTabResponse>();
            foreach (var doc in result)
            {
                var workflow =
                    await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(doc.DocumentWorkflowStatuses?.FirstOrDefault()
                        ?.WorkflowId);
                var user = await _unitOfWork.UserUOW.FindUserByIdAsync(doc.UserId);
                if (user == null)
                    return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                var documentResponse = new DocumentTabResponse
                {
                    DocumentDto = doc,
                    WorkflowName = workflow?.WorkflowName,
                    FullName = user.FullName,
                    Scope = workflow?.Scope,
                };
                documentResponses.Add(documentResponse);
            }

            return ResponseUtil.GetCollection(documentResponses, ResponseMessages.GetSuccessfully, HttpStatusCode.OK,
                totalRecords, page, limit, totalPages);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    private async Task<List<Tasks>> GetOrderedTasks(List<Tasks> tasks, Guid workflowId)
    {
        var flowNumberMap = new Dictionary<(Guid workflowId, Guid flowId), int>();

        async Task<int> GetFlowNumber(Guid workflowId, Guid flowId)
        {
            if (!flowNumberMap.TryGetValue((workflowId, flowId), out var number))
            {
                var wfFlow =
                    await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAndFlowIdAsync(workflowId, flowId);
                number = wfFlow?.FlowNumber ?? int.MaxValue;
                flowNumberMap[(workflowId, flowId)] = number;
            }

            return number;
        }

        var tasksWithFlowNumbers = new List<(Tasks Task, int FlowNumber, int StepNumber, int TaskNumber)>();

        foreach (var t in tasks)
        {
            var flowId = t.Step?.Flow?.FlowId ?? Guid.Empty;
            var flowNumber = await GetFlowNumber(workflowId, flowId);
            var stepNumber = t.Step?.StepNumber ?? int.MaxValue;
            var taskNumber = t.TaskNumber;

            tasksWithFlowNumbers.Add((t, flowNumber, stepNumber, taskNumber));
        }

        var orderedTasks = tasksWithFlowNumbers
            .OrderBy(x => x.FlowNumber)
            .ThenBy(x => x.StepNumber)
            .ThenBy(x => x.TaskNumber)
            .Select(x => x.Task)
            .ToList();

        // Debug: show flow info
        foreach (var x in orderedTasks)
        {
            var flowNumber = x.Step?.Flow?.WorkflowFlows?.FirstOrDefault(wf => wf.WorkflowId == workflowId)?.FlowNumber;
            Console.WriteLine(
                $"TaskId: {x.TaskId}, FlowNumber: {flowNumber}, StepNumber: {x.Step?.StepNumber}, TaskNumber: {x.TaskNumber}");
        }

        return orderedTasks;
    }


    public async Task<ResponseDto> HandleTaskActionAsync(Guid taskId, Guid userId, TaskAction action)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(taskId);

            if (task == null || task.UserId != userId)
                return ResponseUtil.Error(ResponseMessages.TaskNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            var documentTask = task.Document;
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentTask!.DocumentId);
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var orderedTasks = await GetOrderedTasks(document.Tasks.Where(t => t.IsDeleted == false).ToList(),
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            //var user = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTasks.First().UserId);
            var userTask = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);

            switch (action)
            {
                // case TaskAction.AcceptTask:
                // {
                //     task.TaskStatus = TasksStatus.Waiting;
                //     task.UpdatedDate = DateTime.UtcNow;
                //     await _unitOfWork.SaveChangesAsync();
                //
                //     var notification = _notificationService.CreateTaskAcceptedNotification(task, user.UserId);
                //     await _notificationCollection.CreateNotificationAsync(notification);
                //     await _notificationService.SendPushNotificationMobileAsync(user.FcmToken, notification);
                //     await _hubContext.Clients.User(notification.UserId.ToString())
                //         .SendAsync("ReceiveMessage", notification);
                //
                //     await transaction.CommitAsync();
                //     // Gửi thông báo cho người tạo: "Người A đã nhận xử lý"
                //     return ResponseUtil.GetObject(ResponseMessages.TaskHadAccepted,
                //         ResponseMessages.CreatedSuccessfully,
                //         HttpStatusCode.OK, 1);
                // }
                //
                // case TaskAction.RejectTask:
                // {
                //     task.TaskStatus = TasksStatus.Revised;
                //     task.UpdatedDate = DateTime.UtcNow;
                //     await _unitOfWork.SaveChangesAsync();
                //
                //     var notification = _notificationService.CreateTaskRejectedNotification(task, user.UserId);
                //     await _notificationCollection.CreateNotificationAsync(notification);
                //     await _notificationService.SendPushNotificationMobileAsync(user.FcmToken, notification);
                //     await _hubContext.Clients.User(notification.UserId.ToString())
                //         .SendAsync("ReceiveMessage", notification);
                //
                //     await transaction.CommitAsync();
                //     // Gửi thông báo cho người tạo: "Người A từ chối xử lý"
                //     return ResponseUtil.GetObject(ResponseMessages.TaskHadRejected,
                //         ResponseMessages.CreatedSuccessfully,
                //         HttpStatusCode.OK, 1);
                // }

                case TaskAction.ApproveDocument:
                {
                    if (task.TaskStatus != TasksStatus.InProgress)
                        return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);

                    var tasksInSameStep = task.Step.Tasks.Where(d => d.DocumentId == task.DocumentId)
                        .OrderBy(t => t.TaskNumber).ToList();
                    var isMyTurn = tasksInSameStep
                        .Where(t => t.TaskNumber < task.TaskNumber)
                        .All(t => t.TaskStatus == TasksStatus.Completed);

                    if (!isMyTurn)
                        return ResponseUtil.Error(ResponseMessages.TaskHadNotCompleted,
                            ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);

                    task.TaskStatus = TasksStatus.Completed;
                    task.UpdatedDate = DateTime.UtcNow;
                    
                    //TODO: Ky Nhay 
                    if (userTask == null)
                    {
                        return ResponseUtil.Error("User not found", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }

                    if (userTask.DigitalCertificates == null)
                    {
                        return ResponseUtil.Error("User's digital certificates not found", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }
                    var cer = userTask.DigitalCertificates.FirstOrDefault(x => x.IsUsb == null);
                    var documentSignId = Guid.NewGuid();
                    if (document.DocumentVersions == null)
                    {
                        return ResponseUtil.Error("Document versions not found", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }
                    

                    var latestVer = document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion);


                    if (latestVer == null)
                    {
                        return ResponseUtil.Error("Latest version not found", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }

                    if (latestVer.DocumentSignatures == null)
                    {
                        return ResponseUtil.Error("Document signatures not found", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }

                    if (cer == null)
                    {
                        return ResponseUtil.Error("Digital certificate not found", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }
                    var documentSign = new DocumentSignature()
                    {
                        DocumentSignatureId = documentSignId,
                        SignedAt = DateTime.Now,
                        OrderIndex = latestVer.DocumentSignatures.Count + 1,
                        DigitalCertificateId = cer.DigitalCertificateId,
                        DocumentVersionId = latestVer.DocumentVersionId,
                    };
                    
                    await _unitOfWork.DocumentSignatureUOW.AddAsync(documentSign); 
                    await _unitOfWork.TaskUOW.UpdateAsync(task);
                    await _unitOfWork.SaveChangesAsync();

                    // var nextTask = tasksInSameStep.FirstOrDefault(t => t.TaskNumber > task.TaskNumber);
                    // if (nextTask != null)
                    // {
                    //     // TODO: Gửi thông báo cho nextTask.UserId: "Đến lượt bạn duyệt"
                    //     return ResponseUtil.GetObject(ResponseMessages.TaskApproved, ResponseMessages.CreatedSuccessfully,
                    //         HttpStatusCode.OK, 1);
                    // }
                    //
                    // return await PromoteToNextStepOrFlow(task.Step, task.DocumentId!.Value);
                    //return ResponseUtil.GetObject(ResponseMessages.TaskApproved, ResponseMessages.CreatedSuccessfully,
                    //HttpStatusCode.OK, 1);
                    var result = await ActivateNextTask(task);
                    if (result.StatusCode != 200)
                    {
                        await transaction.RollbackAsync();
                    }else
                    {
                        await transaction.CommitAsync();
                    }
                    return result;
                }

                case TaskAction.RejectDocument:
                {
                    if (task.TaskStatus != TasksStatus.InProgress)
                        return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);

                    // 1. Cập nhật trạng thái task hiện tại
                    task.TaskStatus = TasksStatus.Completed;
                    task.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.TaskUOW.UpdateAsync(task);
                    await _unitOfWork.SaveChangesAsync();
                    // 2. Cập nhật trạng thái tài liệu
                    //var document = task.Document;
                    if (document == null)
                        return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);

                    //document.ProcessingStatus = ProcessingStatus.Rejected;
                    //document.UpdatedDate = DateTime.UtcNow;
                    //await _unitOfWork.DocumentUOW.UpdateAsync(document);
                    //await _unitOfWork.SaveChangesAsync();
                    var documentVersions =
                        await _unitOfWork.DocumentVersionUOW.FindDocumentVersionByDocumentIdAsync(document.DocumentId);
                    if (documentVersions != null)
                    {
                        var documentVersion =
                            documentVersions.OrderByDescending(dv => dv.VersionNumber).FirstOrDefault();
                        documentVersion.IsFinalVersion = false;
                        // var documentVersionTemplVersion =
                        //     documentVersions.Where(dv => dv.VersionNumber.Equals("0")).FirstOrDefault();
                        // documentVersionTemplVersion.IsFinalVersion = true;
                        
                        await _unitOfWork.DocumentVersionUOW.UpdateAsync(documentVersion);
                       // await _unitOfWork.DocumentVersionUOW.UpdateAsync(documentVersionTemplVersion);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // 3. Hủy tất cả task còn lại (nếu chưa xử lý)
                    // var allPendingTasks =
                    //     await _unitOfWork.TaskUOW.FindAllPendingTaskByDocumentIdAsync(task.DocumentId!.Value);
                    //
                    // foreach (var pendingTask in allPendingTasks)
                    // {
                    //     pendingTask.TaskStatus =
                    //         TasksStatus.Revised; // Hoặc đặt là Rejected nếu bạn muốn thể hiện bị từ chối
                    //     pendingTask.UpdatedDate = DateTime.UtcNow;
                    //     await _unitOfWork.TaskUOW.UpdateAsync(pendingTask);
                    // }

                    var firstTask = orderedTasks[0].TaskId;

                    foreach (var orderedTask in orderedTasks)
                    {
                        var userFinal = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTask.UserId);
                        var notification = _notificationService.CreateDocRejectedNotification(task, orderedTask.UserId);
                        await _notificationCollection.CreateNotificationAsync(notification);
                        await _notificationService.SendPushNotificationMobileAsync(userFinal.FcmToken, notification);
                        await _hubContext.Clients.User(orderedTask.UserId.ToString())
                            .SendAsync("ReceiveMessage", notification);
                        
                        
                        if (orderedTask.TaskId == firstTask)
                        {
                            orderedTask.TaskStatus = TasksStatus.InProgress;
                            await _unitOfWork.TaskUOW.UpdateAsync(orderedTask);
                        }
                        else
                        {
                            orderedTask.TaskStatus = TasksStatus.Revised;
                            orderedTask.StartDate = DateTime.MinValue;
                            orderedTask.EndDate = DateTime.MinValue;
                            orderedTask.UpdatedDate = DateTime.UtcNow;
                            await _unitOfWork.TaskUOW.UpdateAsync(orderedTask);
                        }
                        
                    }

                    await _unitOfWork.SaveChangesAsync();
                    // 4. Gửi thông báo
                    // TODO: Gửi thông báo cho người tạo tài liệu + người liên quan: "Tài liệu đã bị từ chối ở bước XYZ bởi User A"
                    await transaction.CommitAsync();
                    await _loggingService.WriteLogAsync(userId,$"Văn bản bị từ chối bởi: {userTask.FullName}");
                    return ResponseUtil.GetObject(ResponseMessages.DocumentRejected, ResponseMessages.CreatedSuccessfully,
                        HttpStatusCode.OK, 1);
                }

                case TaskAction.SubmitDocument:
                {
                    if (task.TaskStatus != TasksStatus.InProgress)
                        return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);

                    var tasksInSameStep = task.Step.Tasks
                        .Where(d => d.DocumentId == task.DocumentId && d.IsDeleted == false).OrderBy(t => t.TaskNumber)
                        .ToList();
                    var isMyTurn = tasksInSameStep
                        .Where(t => t.TaskNumber < task.TaskNumber)
                        .All(t => t.TaskStatus == TasksStatus.Completed);

                    if (!isMyTurn)
                        return ResponseUtil.Error(ResponseMessages.TaskHadNotCompleted,
                            ResponseMessages.OperationFailed,
                            HttpStatusCode.BadRequest);

                    var workflow = await _unitOfWork.WorkflowUOW
                        .FindWorkflowByIdAsync(document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId);
                    if (workflow.Scope != Scope.InComing)
                    {
                        if (orderedTasks[1].TaskType != TaskType.Upload)
                        {
                            return ResponseUtil.Error("Task 2 step 1 must be upload", ResponseMessages.OperationFailed,
                                         HttpStatusCode.NotFound);
                        }

                        if (document.ProcessingStatus == ProcessingStatus.Rejected)
                        {
                            document.ProcessingStatus = ProcessingStatus.InProgress;
                            document.UpdatedDate = DateTime.UtcNow;
                            await _unitOfWork.DocumentUOW.UpdateAsync(document);
                            await _unitOfWork.SaveChangesAsync();
                        }
                        
                        var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflow.WorkflowId);
                    
                        var allStepsWithFlowNumber = workflowFlowAll
                            .SelectMany(wf => wf.Flow.Steps.Select(s => new
                            {
                                Step = s,
                                FlowNumber = wf.FlowNumber,
                                StepNumber = s.StepNumber
                            }))
                            .ToList();

                        // Kiểm tra tất cả các step trong tất cả các flow
                        foreach (var stepInfo in allStepsWithFlowNumber)
                        {
                            var tasksInStep = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(stepInfo.Step.StepId, task.DocumentId);
                            if (!tasksInStep.Any())
                            {
                                return ResponseUtil.Error(
                                    "There is a step without task",
                                    ResponseMessages.OperationFailed,
                                    HttpStatusCode.BadRequest);
                            }
                        }
                        
                    }
                    else
                    {
                        // var workflowFlowAll = (await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflow.WorkflowId)).OrderBy(w => w.FlowNumber);
                        //
                        // // Tạo danh sách allStepsWithFlowNumber bằng foreach
                        // var allStepsWithFlowNumber = new List<object>();
                        // foreach (var wf in workflowFlowAll)
                        // {
                        //     if (wf.Flow != null && wf.Flow.Steps != null)
                        //     {
                        //         foreach (var step in wf.Flow.Steps)
                        //         {
                        //             allStepsWithFlowNumber.Add(new
                        //             {
                        //                 Step = step,
                        //                 FlowNumber = wf.FlowNumber,
                        //                 StepNumber = step.StepNumber
                        //             });
                        //         }
                        //     }
                        // }
                        //
                        // // Kiểm tra step đầu tiên có role chief
                        // object firstChiefStep = null;
                        // foreach (var s in allStepsWithFlowNumber)
                        // {
                        //     var step = await _unitOfWork.StepUOW.FindStepByIdAsync(((dynamic)s).Step.StepId);
                        //     if (step.Role != null && step.Role.RoleName.Equals("chief", StringComparison.OrdinalIgnoreCase))
                        //     {
                        //         if (firstChiefStep == null ||
                        //             ((dynamic)s).FlowNumber < ((dynamic)firstChiefStep).FlowNumber ||
                        //             (((dynamic)s).FlowNumber == ((dynamic)firstChiefStep).FlowNumber && ((dynamic)s).StepNumber < ((dynamic)firstChiefStep).StepNumber))
                        //         {
                        //             firstChiefStep = s;
                        //         }
                        //     }
                        // }
                        //
                        // if (firstChiefStep != null)
                        // {
                        //     var step = ((dynamic)firstChiefStep).Step;
                        //     var tasksInChiefStep = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(step.StepId, task.DocumentId);
                        //     bool hasInvalidTask = false;
                        //     foreach (var t in tasksInChiefStep)
                        //     {
                        //         if (t.UserId != task.UserId && t.TaskStatus == TasksStatus.InProgress)
                        //         {
                        //             hasInvalidTask = true;
                        //             break;
                        //         }
                        //     }
                        //     if (hasInvalidTask)
                        //     {
                        //         return ResponseUtil.Error(
                        //             "Người dùng có role chief chỉ được phép có duy nhất một task Create trong tài liệu.",
                        //             ResponseMessages.OperationFailed,
                        //             HttpStatusCode.BadRequest);
                        //     }
                        // }

                        // Kiểm tra user với role chief chỉ có một task Create
                        var tasksInDocument = document.Tasks.Where(t => t.IsDeleted == false).ToList();
                        var userCheckIds = new List<Guid>();
                        foreach (var t in tasksInDocument)
                        {
                            if (!t.IsDeleted && !userCheckIds.Contains(t.UserId))
                            {
                                userCheckIds.Add(t.UserId);
                            }
                        }
                        foreach (var userCheckId in userCheckIds)
                        {
                            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userCheckId);
                            bool hasChiefRole = false;
                            foreach (var ur in user.UserRoles)
                            {
                                if (ur.Role.RoleName.Equals("chief", StringComparison.OrdinalIgnoreCase))
                                {
                                    hasChiefRole = true;
                                    break;
                                }
                            }
                            if (hasChiefRole)
                            {
                                int createTaskCount = 0;
                                foreach (var t in tasksInDocument)
                                {
                                    if (t.UserId == userCheckId && t.TaskType == TaskType.Create && !t.IsDeleted)
                                    {
                                        createTaskCount++;
                                    }
                                }
                                if (createTaskCount != 1)
                                {
                                    return ResponseUtil.Error(
                                        "Người dùng có role chief phải có đúng một task Create trong tài liệu.",
                                        ResponseMessages.OperationFailed,
                                        HttpStatusCode.BadRequest);
                                }
                            }
                        }

                       if (document.FinalArchiveDocument == null)
                        {
                            return ResponseUtil.Error("Final archive document not found", ResponseMessages.OperationFailed,
                                HttpStatusCode.NotFound);
                        }
                       
                        var permissions = document.FinalArchiveDocument.UserDocumentPermissions;
                        if (permissions == null)
                        {
                            return ResponseUtil.Error("User document permissions not found", ResponseMessages.OperationFailed,
                                HttpStatusCode.NotFound);
                        }

                        foreach (var taskE in orderedTasks)
                        {
                            var isInPermission = false;
                            foreach (var userDocumentPermission in permissions)
                            {
                                if (userDocumentPermission.UserId == taskE.UserId)
                                {
                                    isInPermission = true;
                                    break;
                                }
                                if (orderedTasks.Any(x => x.UserId == userDocumentPermission.UserId)) continue;
                                userDocumentPermission.IsDeleted = true;
                                await _unitOfWork.UserDocPermissionUOW.UpdateAsync(userDocumentPermission);
                            }

                            var archiveId = document.FinalArchiveDocumentId;
                            if (archiveId == null)
                            {
                                return ResponseUtil.Error("ArchiveId not found", ResponseMessages.OperationFailed,
                                    HttpStatusCode.NotFound);
                            }

                            if (isInPermission) continue;
                            var per = new UserDocumentPermission()
                            {
                                CreatedDate = DateTime.Now,
                                GrantPermission = GrantPermission.Grant,
                                IsDeleted = false,
                                UserId = taskE.UserId,
                                ArchivedDocumentId = (Guid)archiveId,
                            };
                            if(per.ArchivedDocumentId == Guid.Empty) 
                            {
                                return ResponseUtil.Error("ArchiveId not found", ResponseMessages.OperationFailed,
                                    HttpStatusCode.NotFound);
                            }
                            await _unitOfWork.UserDocPermissionUOW.AddAsync(per);
                        }
                    }

                    foreach (var orderedTask in orderedTasks.Where(orderedTask => orderedTask.StartDate == DateTime.MinValue))
                    {
                        return ResponseUtil.Error($"Please update start date and end date of task: {orderedTask.Title}", ResponseMessages.OperationFailed,
                            HttpStatusCode.NotFound);
                    }
                    
                    
                    


                    task.TaskStatus = TasksStatus.Completed;
                    task.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.TaskUOW.UpdateAsync(task);
                    await _unitOfWork.SaveChangesAsync();

                    var result = await ActivateNextTaskSubmit(task);
                    if (result.StatusCode != 200)
                    {
                        await transaction.RollbackAsync();
                    }else
                    {
                        await transaction.CommitAsync();
                    }
                    
                    return result;
                }
            }

            return ResponseUtil.Error(ResponseMessages.InvalidAction, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
    }


    private async Task<ResponseDto> ActivateNextTask(Tasks currentTask)
    {
        var currentStep = currentTask.Step;
        var stepp = await _unitOfWork.StepUOW.FindStepByIdAsync(currentStep.StepId);
        var currentFlow = await _unitOfWork.FlowUOW.FindFlowByIdAsync(stepp.FlowId);
        var documentId = currentTask.DocumentId;

        // 🔍 Tìm task kế tiếp trong cùng Step
        var tasksInStep = stepp.Tasks.Where(d => d.DocumentId == documentId && d.IsDeleted == false)
            .OrderBy(t => t.TaskNumber).ToList();
        var nextTask = tasksInStep.FirstOrDefault(t => t.TaskNumber > currentTask.TaskNumber);
        var previousTask = tasksInStep
            .Where(t => t.TaskNumber < currentTask.TaskNumber)
            .OrderByDescending(t => t.TaskNumber)
            .FirstOrDefault();

        if (previousTask != null)
        {
            var preUser = await _unitOfWork.UserUOW.FindUserByIdAsync(previousTask.UserId);
            var notification = _notificationService.CreateDocAcceptedNotification(previousTask, previousTask.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _notificationService.SendPushNotificationMobileAsync(preUser.FcmToken, notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
        }

        if (nextTask != null)
        {
            nextTask.TaskStatus = TasksStatus.InProgress;
            nextTask.UpdatedDate = DateTime.UtcNow;

            // TODO: Gửi thông báo đến nextTask.UserId
            var nextUser = await _unitOfWork.UserUOW.FindUserByIdAsync(nextTask.UserId);
            var notification = _notificationService.CreateNextUserDoTaskNotification(nextTask, nextTask.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _notificationService.SendPushNotificationMobileAsync(nextUser.FcmToken, notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
            await _unitOfWork.TaskUOW.UpdateAsync(nextTask);
            await _unitOfWork.SaveChangesAsync();

            return ResponseUtil.GetObject($"Đến lượt duyệt tiếp theo:{nextTask.UserId}",
                ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        // Không còn task trong Step hiện tại — kiểm tra Step tiếp theo
        var stepsInFlow = currentFlow.Steps.OrderBy(s => s.StepNumber).ToList();
        var currentStepIndex = stepsInFlow.FindIndex(s => s.StepId == currentStep.StepId);

        if (currentStepIndex < stepsInFlow.Count - 1)
        {
            var nextStep = stepsInFlow[currentStepIndex + 1];
            var nextStepTasks =
                (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(nextStep.StepId, documentId.Value))
                .OrderBy(t => t.TaskNumber).ToList();
            var firstTaskInNextStep = nextStepTasks.FirstOrDefault();
            var finalTaskInCurrentStep = tasksInStep.OrderByDescending(t => t.TaskNumber).FirstOrDefault();
            if (finalTaskInCurrentStep != null)
            {
                var finalTaskUser = await _unitOfWork.UserUOW.FindUserByIdAsync(finalTaskInCurrentStep.UserId);
                var notification =
                    _notificationService.CreateDocAcceptedNotification(finalTaskInCurrentStep,
                        finalTaskInCurrentStep.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(finalTaskUser.FcmToken, notification);
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveMessage", notification);
            }

            if (firstTaskInNextStep != null)
            {
                firstTaskInNextStep.TaskStatus = TasksStatus.InProgress;
                firstTaskInNextStep.UpdatedDate = DateTime.UtcNow;

                var firstTaskUser = await _unitOfWork.UserUOW.FindUserByIdAsync(firstTaskInNextStep.UserId);
                // TODO: Gửi thông báo
                var notification =
                    _notificationService.CreateNextUserDoTaskNotification(firstTaskInNextStep,
                        firstTaskInNextStep.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(firstTaskUser.FcmToken, notification);
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveMessage", notification);
                await _unitOfWork.TaskUOW.UpdateAsync(firstTaskInNextStep);
                await _unitOfWork.SaveChangesAsync();

                return ResponseUtil.GetObject($"Chuyển sang bước tiếp theo: {firstTaskInNextStep.UserId}",
                    ResponseMessages.CreatedSuccessfully,
                    HttpStatusCode.OK, 1);
            }
        }

        var docWorkStatusId = currentTask.Document?.DocumentWorkflowStatuses?.FirstOrDefault().DocumentWorkflowStatusId;
        var docWorkStatus = await _unitOfWork.DocumentWorkflowStatusUOW
            .FindDocumentWorkflowStatusByIdAsync(docWorkStatusId);
        
        // var workflowFlow = await _unitOfWork.WorkflowFlowUOW
        //     .FindWorkflowFlowByFlowIdAsync(currentFlow.FlowId); // Hoặc FlowId thôi nếu đủ
        //
        // if (workflowFlow == null)
        //     return ResponseUtil.Error("Không tìm thấy WorkflowFlow", ResponseMessages.OperationFailed,
        //         HttpStatusCode.BadRequest);
        //
        // var workflowId = workflowFlow.WorkflowId;
        
        var workflowId = docWorkStatus.WorkflowId;

        // Step hiện tại là cuối cùng của Flow — kiểm tra Flow kế tiếp
        return await ActivateFirstTaskOfNextFlow(workflowId, currentFlow, documentId.Value);
    }

    private async Task<ResponseDto> ActivateFirstTaskOfNextFlow(Guid workflowId, Flow currentFlow, Guid documentId)
    {
        // Lấy tất cả WorkflowFlow của Workflow hiện tại, theo thứ tự
        var workflowFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
        var orderedWorkflowFlows = workflowFlows.OrderBy(wf => wf.FlowNumber).ToList();
        var task = new Tasks();
        var currentWorkflowFlowIndex = orderedWorkflowFlows.FindIndex(wf => wf.FlowId == currentFlow.FlowId);
        var currentWorkflowFlow = orderedWorkflowFlows.Where(wf => wf.FlowId == currentFlow.FlowId).FirstOrDefault();
        if (currentWorkflowFlowIndex < orderedWorkflowFlows.Count - 1)
        {
            var nextWorkflowFlow = orderedWorkflowFlows[currentWorkflowFlowIndex + 1];
            var nextFlow = nextWorkflowFlow.Flow;
            var nextSteps = nextFlow.Steps.OrderBy(s => s.StepNumber).ToList();
            var firstSteps = nextSteps.FirstOrDefault();
            var firstStep = await _unitOfWork.StepUOW.FindStepByIdAsync(firstSteps.StepId);
            var finalSteps = currentWorkflowFlow.Flow.Steps.OrderByDescending(s => s.StepNumber).ToList();
            var finalStep = finalSteps.FirstOrDefault();
            if (finalStep != null)
            {
                var finalTask = (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(finalStep.StepId, documentId))
                    .OrderByDescending(t => t.TaskNumber)
                    .FirstOrDefault();
                if (finalTask != null)
                {
                    var finalUser = await _unitOfWork.UserUOW.FindUserByIdAsync(finalTask.UserId);
                    var notification = _notificationService.CreateDocAcceptedNotification(finalTask, finalTask.UserId);
                    await _notificationCollection.CreateNotificationAsync(notification);
                    await _notificationService.SendPushNotificationMobileAsync(finalUser.FcmToken, notification);
                    await _hubContext.Clients.User(notification.UserId.ToString())
                        .SendAsync("ReceiveMessage", notification);
                }
            }

            if (firstStep != null)
            {
                var firstTask = (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(firstStep.StepId, documentId))
                    .OrderBy(t => t.TaskNumber)
                    .FirstOrDefault();
                task = firstTask;
                if (firstTask != null)
                {
                    firstTask.TaskStatus = TasksStatus.InProgress;
                    firstTask.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.TaskUOW.UpdateAsync(firstTask);

                    // TODO: Gửi thông báo
                    await _unitOfWork.SaveChangesAsync();

                    var documentWorkflowflowStatus =
                        await _unitOfWork.DocumentWorkflowStatusUOW
                            .FindDocumentWorkflowStatusByWorkflowIdWorkflowFlowIdDocIdAsync(workflowId,
                                currentWorkflowFlow!.WorkflowFlowId, documentId);
                    if (documentWorkflowflowStatus != null)
                    {
                        documentWorkflowflowStatus.StatusDocWorkflow = StatusDocWorkflow.Approval;
                        documentWorkflowflowStatus.StatusDoc = StatusDoc.Approval;
                        documentWorkflowflowStatus.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.DocumentWorkflowStatusUOW.UpdateAsync(documentWorkflowflowStatus);

                        var nextDocumentWorkflowflowStatus = new DocumentWorkflowStatus
                        {
                            StatusDocWorkflow = StatusDocWorkflow.Pending,
                            StatusDoc = StatusDoc.Pending,
                            UpdatedAt = DateTime.UtcNow,
                            DocumentId = documentId,
                            WorkflowId = workflowId,
                            CurrentWorkflowFlowId = nextWorkflowFlow.WorkflowFlowId
                        };
                        await _unitOfWork.DocumentWorkflowStatusUOW.AddAsync(nextDocumentWorkflowflowStatus);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    var firstUser = await _unitOfWork.UserUOW.FindUserByIdAsync(firstTask.UserId);
                    var notification =
                        _notificationService.CreateNextUserDoTaskNotification(firstTask, firstTask.UserId);
                    await _notificationCollection.CreateNotificationAsync(notification);
                    await _notificationService.SendPushNotificationMobileAsync(firstUser.FcmToken, notification);
                    await _hubContext.Clients.User(notification.UserId.ToString())
                        .SendAsync("ReceiveMessage", notification);

                    return ResponseUtil.GetObject($"Chuyển sang Flow tiếp theo: {firstTask.UserId}",
                        ResponseMessages.CreatedSuccessfully,
                        HttpStatusCode.OK, 1);
                }
            }
        }

        // Không còn Flow nào → đánh dấu document đã hoàn tất
        var doc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);


        if (doc == null)
            return ResponseUtil.Error("Không tìm thấy tài liệu", ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        {
            doc.ProcessingStatus = ProcessingStatus.Completed;
            doc.UpdatedDate = DateTime.UtcNow;
            
            //TODO: Minh archive

            
            var latestVer = doc.DocumentVersions?.FirstOrDefault(x => x.IsFinalVersion);
            if (latestVer is not { DocumentSignatures: not null })
            {
                return ResponseUtil.Error("DocumentSignatures not found",
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            
            
            var signByList = latestVer.DocumentSignatures.Select(x => x.DigitalCertificate).Select(x => x.User);
            var signBy = string.Join(", ", signByList.Select(x => x.FullName));
            if (doc.DocumentWorkflowStatuses == null)
            {
                return ResponseUtil.Error("DocumentWorkflowStatuses not found",
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            if (doc.User == null)
            {
                return ResponseUtil.Error("User not found",
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
                
                
            var userList = new List<User> { doc.User };
            if (doc.Tasks != null) userList.AddRange(doc.Tasks.Select(x => x.User)!);
            

            var scopeDoc = doc.DocumentWorkflowStatuses.Select(x => x.Workflow).FirstOrDefault()!.Scope;
            
            if (doc.DocumentName == null)
            {
                return ResponseUtil.Error("DocumentName not found",
                    ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
       
            
            var archivedDoc = new ArchivedDocument();
            var orderedTasks = await GetOrderedTasks(doc.Tasks,
                doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            if(doc.FinalArchiveDocumentId==null)
            {
                var archivedDocId = Guid.NewGuid();
                var url = _fileService.ArchiveDocument(doc.DocumentName, doc.DocumentId,
                    latestVer.DocumentVersionId,
                    archivedDocId);
                // var userPermission = userList.Select(x =>
                //     {
                //         var userPermission = new UserDocumentPermission()
                //         {
                //             UserId = x.UserId,
                //             CreatedDate = DateTime.Now,
                //             IsDeleted = false,
                //             ArchivedDocumentId = archivedDocId,
                //             GrantPermission = GrantPermission.Grant
                //         };
                //         return userPermission;
                //     }
                // ).ToList();
                
                var userPermissions = new List<UserDocumentPermission>();

                
                var distinctUserIds = orderedTasks
                    .Select(t => t.UserId)
                    .Distinct()
                    .ToList();

                var workFlowO = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
                
                

                foreach (var userId in distinctUserIds)
                {
                    var exists = await _unitOfWork.UserDocPermissionUOW.ExistsAsync(userId, archivedDocId);
                    if (!exists)
                    {
                        userPermissions.Add(new UserDocumentPermission
                        {
                            UserId = userId,
                            ArchivedDocumentId = archivedDocId,
                            GrantPermission = GrantPermission.Grant,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }

                if (workFlowO.Scope == Scope.School)
                {
                    var liU = await _unitOfWork.UserUOW.FindAllUserAsync();
                    liU = liU.Where(x => x.IsDeleted == false && !x.Email.Contains("admin") && !distinctUserIds.Contains(x.UserId)).ToList();
                    foreach (var userId in liU.Select(x => x.UserId).ToList())
                    {
                        var exists = await _unitOfWork.UserDocPermissionUOW.ExistsAsync(userId, archivedDocId);
                        if (!exists)
                        {
                            userPermissions.Add(new UserDocumentPermission
                            {
                                UserId = userId,
                                ArchivedDocumentId = archivedDocId,
                                GrantPermission = GrantPermission.View,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                    }
                }
                else if (workFlowO.Scope == Scope.Division)
                {
                    var liU = orderedTasks.FirstOrDefault().User.Division.Users;
                    liU = liU.Where(x => x.IsDeleted == false && !x.Email.Contains("admin") && !distinctUserIds.Contains(x.UserId)).ToList();
                    foreach (var userId in liU.Select(x => x.UserId).ToList())
                    {
                        var exists = await _unitOfWork.UserDocPermissionUOW.ExistsAsync(userId, archivedDocId);
                        if (!exists)
                        {
                            userPermissions.Add(new UserDocumentPermission
                            {
                                UserId = userId,
                                ArchivedDocumentId = archivedDocId,
                                GrantPermission = GrantPermission.Download,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                    }
                }
                // Chỉ thêm những cái chưa có
                if (userPermissions.Any())
                {
                    await _unitOfWork.UserDocPermissionUOW.AddRangeAsync(userPermissions);
                }

                //await _unitOfWork.SaveChangesAsync();
                var listSignature = latestVer.DocumentSignatures;
                // foreach (var userDocumentPermission in userPermission)
                // {
                //     await _unitOfWork.UserDocPermissionUOW.AddAsync(userDocumentPermission);
                // }
                var listSignArchive = listSignature.Select(x =>
                    {
                        var signArchive = new ArchiveDocumentSignature()
                        {
                            DigitalCertificateId = x.DigitalCertificateId,
                            SignedAt = x.SignedAt,
                            OrderIndex = x.OrderIndex,
                            ArchivedDocumentId = archivedDocId
                        };
                        return signArchive;
                    }
                ).ToList();
                foreach (var archiveDocumentSignature in listSignArchive)
                {
                    await _unitOfWork.ArchiveDocumentSignatureUOW.AddAsync(archiveDocumentSignature);
                }
                var attachDoc = doc.AttachmentDocuments;
                // List<AttachmentArchivedDocument> attachmentArchived =[];
                foreach (var attachmentDocument in attachDoc)
                {
                    var att = new AttachmentArchivedDocument()
                    {
                        //AttachmentArchivedDocumentId = archivedDocId,
                        AttachmentName = attachmentDocument.AttachmentDocumentName,
                        AttachmentUrl = attachmentDocument.AttachmentDocumentUrl,
                        ArchivedDocumentId = archivedDocId,
                    };
                    await _unitOfWork.AttachmentArchivedUOW.AddAsync(att);
                }
                archivedDoc = new ArchivedDocument()
                {
                    ArchivedDocumentId = archivedDocId,
                    ArchivedDocumentName = doc.DocumentName,
                    ArchivedDocumentContent = doc.DocumentContent,
                    NumberOfDocument = doc.NumberOfDocument,
                    SystemNumberOfDoc = doc.SystemNumberOfDoc,
                    SignedBy = signBy,
                    ArchivedDocumentUrl = url,
                    CreatedDate = DateTime.Now,
                    ExpirationDate = doc.ExpirationDate,
                    Sender = null,
                    CreatedBy = doc.User.FullName,
                    ExternalPartner = null,
                    ArchivedDocumentStatus = ArchivedDocumentStatus.Archived,
                    DateIssued = doc.DateIssued.Equals(DateTime.MaxValue) || doc.DateIssued.Equals(DateTime.MinValue) || doc.DateIssued == null ? DateTime.Now : doc.DateIssued,
                    DateReceived = null,
                    DateSented = null,
                    DocumentRevokes = null,
                    DocumentReplaces = null,
                    //Scope = scopeDoc,
                    Scope = (await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId)).Scope,
                    IsTemplate = false,
                    DocumentTypeId = doc.DocumentTypeId ?? Guid.NewGuid(),
                    FinalDocumentId = doc.DocumentId
                };
                await _unitOfWork.ArchivedDocumentUOW.AddAsync(archivedDoc);
            }
            else
            {
                archivedDoc = doc.FinalArchiveDocument;
                if (archivedDoc == null)
                    return ResponseUtil.Error("Final archive not found", ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                var attachDoc = doc.AttachmentDocuments;
                // List<AttachmentArchivedDocument> attachmentArchived =[];
                foreach (var attachmentDocument in attachDoc)
                {
                    var att = new AttachmentArchivedDocument()
                    {
                        //AttachmentArchivedDocumentId = archivedDoc.ArchivedDocumentId,
                        AttachmentName = attachmentDocument.AttachmentDocumentName,
                        AttachmentUrl = attachmentDocument.AttachmentDocumentUrl,
                        ArchivedDocumentId = archivedDoc.ArchivedDocumentId,
                    };
                    await _unitOfWork.AttachmentArchivedUOW.AddAsync(att);
                }
                archivedDoc.ArchivedDocumentName = doc.DocumentName;
                archivedDoc.SystemNumberOfDoc = doc.SystemNumberOfDoc;
                archivedDoc.ArchivedDocumentContent = doc.DocumentContent;
                archivedDoc.NumberOfDocument = doc.NumberOfDocument;
                archivedDoc.SignedBy = signBy;
                archivedDoc.ArchivedDocumentUrl =
                    $"{_host}/api/Document/view-file/{archivedDoc.ArchivedDocumentId}?version=1&isArchive=true";
                archivedDoc.ExpirationDate = doc.ExpirationDate;
                archivedDoc.CreatedBy = doc.User.UserName;
                archivedDoc.ArchivedDocumentStatus = ArchivedDocumentStatus.Archived;
                archivedDoc.Scope = (await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId)).Scope;
                archivedDoc.DateIssued =
                    doc.DateIssued.Equals(DateTime.MaxValue) || doc.DateIssued.Equals(DateTime.MinValue) ||
                    doc.DateIssued == null
                        ? DateTime.Now
                        : doc.DateIssued;
                ;
                archivedDoc.DocumentType = doc.DocumentType;
                archivedDoc.FinalDocumentId = doc.DocumentId;
                        
                // doc.FinalArchiveDocumentId = archiveId;
                doc.IsDeleted = true;
                doc.ProcessingStatus = ProcessingStatus.Archived;
                await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(archivedDoc);
            }

            var docSign = latestVer.DocumentSignatures;
           
            doc.ProcessingStatus = ProcessingStatus.Archived;
            doc.FinalArchiveDocumentId = archivedDoc.ArchivedDocumentId;
            doc.IsDeleted = true;
            await _unitOfWork.DocumentUOW.UpdateAsync(doc);
            // await _unitOfWork.ArchivedDocumentUOW.AddAsync(archivedDoc);
            // await _unitOfWork.SaveChangesAsync();
            
            
            
            

            foreach (var orderedTask in orderedTasks)
            {
                var orUser = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTask.UserId);
                var notification = _notificationService.CreateDocCompletedNotification(orderedTask, orderedTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(orUser.FcmToken, notification);
                await _hubContext.Clients.User(orderedTask.UserId.ToString()).SendAsync("ReceiveMessage", notification);
            }


            //---------------------------------
            // var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);


            await _unitOfWork.SaveChangesAsync();

            // TODO: Gửi thông báo cho người tạo
            return ResponseUtil.GetObject("Tài liệu đã duyệt xong", ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

    }

    private async Task<ResponseDto> ActivateNextTaskSubmit(Tasks currentTask)
    {
        var currentStep = currentTask.Step;
        var stepp = await _unitOfWork.StepUOW.FindStepByIdAsync(currentStep.StepId);
        var currentFlow = await _unitOfWork.FlowUOW.FindFlowByIdAsync(stepp.FlowId);
        var documentId = currentTask.DocumentId;

        // 🔍 Tìm task kế tiếp trong cùng Step
        var tasksInStep = stepp.Tasks.Where(d => d.DocumentId == documentId && d.IsDeleted == false)
            .OrderBy(t => t.TaskNumber).ToList();
        var nextTask = tasksInStep.FirstOrDefault(t => t.TaskNumber > currentTask.TaskNumber);
        var previousTask = tasksInStep
            .Where(t => t.TaskNumber < currentTask.TaskNumber)
            .OrderByDescending(t => t.TaskNumber)
            .FirstOrDefault();

        if (previousTask != null)
        {
            var preUser = await _unitOfWork.UserUOW.FindUserByIdAsync(previousTask.UserId);
            var notification = _notificationService.CreateDocAcceptedNotification(previousTask, previousTask.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _notificationService.SendPushNotificationMobileAsync(preUser.FcmToken, notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
        }

        if (nextTask != null)
        {
            nextTask.TaskStatus = TasksStatus.InProgress;
            nextTask.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.TaskUOW.UpdateAsync(nextTask);

            // TODO: Gửi thông báo đến nextTask.UserId
            var nextUser = await _unitOfWork.UserUOW.FindUserByIdAsync(nextTask.UserId);
            var notification = _notificationService.CreateNextUserDoTaskNotification(nextTask, nextTask.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _notificationService.SendPushNotificationMobileAsync(nextUser.FcmToken, notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);

            await _unitOfWork.SaveChangesAsync();

            return ResponseUtil.GetObject($"Đến lượt duyệt tiếp theo:{nextTask.UserId}",
                ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        // Không còn task trong Step hiện tại — kiểm tra Step tiếp theo
        var stepsInFlow = currentFlow.Steps.OrderBy(s => s.StepNumber).ToList();
        var currentStepIndex = stepsInFlow.FindIndex(s => s.StepId == currentStep.StepId);

        if (currentStepIndex < stepsInFlow.Count - 1)
        {
            var nextStep = stepsInFlow[currentStepIndex + 1];
            var nextStepTasks =
                (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(nextStep.StepId, documentId.Value))
                .OrderBy(t => t.TaskNumber).ToList();
            var firstTaskInNextStep = nextStepTasks.FirstOrDefault();
            var finalTaskInCurrentStep = tasksInStep.OrderByDescending(t => t.TaskNumber).FirstOrDefault();
            if (finalTaskInCurrentStep != null)
            {
                var finalTaskUser = await _unitOfWork.UserUOW.FindUserByIdAsync(finalTaskInCurrentStep.UserId);
                var notification =
                    _notificationService.CreateDocAcceptedNotification(finalTaskInCurrentStep,
                        finalTaskInCurrentStep.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(finalTaskUser.FcmToken, notification);
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveMessage", notification);
            }

            if (firstTaskInNextStep != null)
            {
                firstTaskInNextStep.TaskStatus = TasksStatus.InProgress;
                firstTaskInNextStep.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.TaskUOW.UpdateAsync(firstTaskInNextStep);

                var firstTaskUser = await _unitOfWork.UserUOW.FindUserByIdAsync(firstTaskInNextStep.UserId);
                // TODO: Gửi thông báo
                var notification =
                    _notificationService.CreateNextUserDoTaskNotification(firstTaskInNextStep,
                        firstTaskInNextStep.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(firstTaskUser.FcmToken, notification);
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveMessage", notification);
                await _unitOfWork.SaveChangesAsync();

                return ResponseUtil.GetObject($"Chuyển sang bước tiếp theo: {firstTaskInNextStep.UserId}",
                    ResponseMessages.CreatedSuccessfully,
                    HttpStatusCode.OK, 1);
            }
        }

        // var workflowFlow = await _unitOfWork.WorkflowFlowUOW
        //     .FindWorkflowFlowByFlowIdAsync(currentFlow.FlowId); // Hoặc FlowId thôi nếu đủ
        //
        // if (workflowFlow == null)
        //     return ResponseUtil.Error("Không tìm thấy WorkflowFlow", ResponseMessages.OperationFailed,
        //         HttpStatusCode.BadRequest);
        //
        // var workflowId = workflowFlow.WorkflowId;
        
        var docWorkStatusId = currentTask.Document?.DocumentWorkflowStatuses?.FirstOrDefault().DocumentWorkflowStatusId;
        var docWorkStatus = await _unitOfWork.DocumentWorkflowStatusUOW
            .FindDocumentWorkflowStatusByIdAsync(docWorkStatusId);
        var workflowId = docWorkStatus.WorkflowId;

        // Step hiện tại là cuối cùng của Flow — kiểm tra Flow kế tiếp
        return await ActivateFirstTaskOfNextFlowSubmit(workflowId, currentFlow, documentId.Value, currentTask);
    }

    private async Task<ResponseDto> ActivateFirstTaskOfNextFlowSubmit(Guid workflowId, Flow currentFlow,
        Guid documentId, Tasks currentTask)
    {
        // Lấy tất cả WorkflowFlow của Workflow hiện tại, theo thứ tự
        var workflowFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
        var orderedWorkflowFlows = workflowFlows.OrderBy(wf => wf.FlowNumber).ToList();
        var task = new Tasks();
        var currentWorkflowFlowIndex = orderedWorkflowFlows.FindIndex(wf => wf.FlowId == currentFlow.FlowId);
        var currentWorkflowFlow = orderedWorkflowFlows.Where(wf => wf.FlowId == currentFlow.FlowId).FirstOrDefault();

        if (currentWorkflowFlowIndex < orderedWorkflowFlows.Count - 1)
        {
            var nextWorkflowFlow = orderedWorkflowFlows[currentWorkflowFlowIndex + 1];
            var nextFlow = nextWorkflowFlow.Flow;
            var nextSteps = nextFlow.Steps.OrderBy(s => s.StepNumber).ToList();
            var firstStep = nextSteps.FirstOrDefault();
            var finalSteps = currentWorkflowFlow.Flow.Steps.OrderByDescending(s => s.StepNumber).ToList();
            var finalStep = finalSteps.FirstOrDefault();
            if (finalStep != null)
            {
                var finalTask = (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(finalStep.StepId, documentId))
                    .OrderByDescending(t => t.TaskNumber)
                    .FirstOrDefault();
                if (finalTask != null)
                {
                    var finalUser = await _unitOfWork.UserUOW.FindUserByIdAsync(finalTask.UserId);
                    var notification = _notificationService.CreateDocAcceptedNotification(finalTask, finalTask.UserId);
                    await _notificationCollection.CreateNotificationAsync(notification);
                    await _notificationService.SendPushNotificationMobileAsync(finalUser.FcmToken, notification);
                    await _hubContext.Clients.User(notification.UserId.ToString())
                        .SendAsync("ReceiveMessage", notification);
                }
            }

            if (firstStep != null)
            {
                var firstTask = (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(firstStep.StepId, documentId))
                    .OrderBy(t => t.TaskNumber)
                    .FirstOrDefault();
                task = firstTask;
                if (firstTask != null)
                {
                    firstTask.TaskStatus = TasksStatus.InProgress;
                    firstTask.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.TaskUOW.UpdateAsync(firstTask);
                    // TODO: Gửi thông báo
                    await _unitOfWork.SaveChangesAsync();

                    var firstUser = await _unitOfWork.UserUOW.FindUserByIdAsync(firstTask.UserId);
                    var notification =
                        _notificationService.CreateNextUserDoTaskNotification(firstTask, firstTask.UserId);
                    await _notificationCollection.CreateNotificationAsync(notification);
                    await _notificationService.SendPushNotificationMobileAsync(firstUser.FcmToken, notification);
                    await _hubContext.Clients.User(notification.UserId.ToString())
                        .SendAsync("ReceiveMessage", notification);

                    return ResponseUtil.GetObject($"Chuyển sang Flow tiếp theo: {firstTask.UserId}",
                        ResponseMessages.CreatedSuccessfully,
                        HttpStatusCode.OK, 1);
                }
            }
        }

        var doc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);


        if (doc != null)
        {
            doc.IsDeleted = true;
            doc.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.DocumentUOW.UpdateAsync(doc);

            var orderedTasks = await GetOrderedTasks(doc.Tasks,
                doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);

            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
            if (workflow.Scope == Scope.InComing && currentTask.TaskType == TaskType.View)
            {
                var distinctUserIds = orderedTasks
                    .Select(t => t.UserId)
                    .Distinct()
                    .ToList();
                var docArchiveId = doc.FinalArchiveDocumentId;
                if (docArchiveId == null)
                {
                    return ResponseUtil.Error(ResponseMessages.DocumentHasNotArchiveDoc,
                        ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
                }

                var userPermissions = new List<UserDocumentPermission>();

                foreach (var userId in distinctUserIds)
                {
                    var exists = await _unitOfWork.UserDocPermissionUOW.ExistsAsync(userId, docArchiveId.Value);
                    if (!exists)
                    {
                        userPermissions.Add(new UserDocumentPermission
                        {
                            UserId = userId,
                            ArchivedDocumentId = docArchiveId.Value,
                            GrantPermission = GrantPermission.Grant,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }

                // Chỉ thêm những cái chưa có
                if (userPermissions.Any())
                {
                    await _unitOfWork.UserDocPermissionUOW.AddRangeAsync(userPermissions);
                }

                await _unitOfWork.SaveChangesAsync();
            }


            foreach (var orderedTask in orderedTasks)
            {
                var orUser = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTask.UserId);
                var notification = _notificationService.CreateDocCompletedNotification(orderedTask, orderedTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(orUser.FcmToken, notification);
                await _hubContext.Clients.User(orderedTask.UserId.ToString()).SendAsync("ReceiveMessage", notification);
            }

            // TODO: (Minh) Viết check document
            if (doc.DocumentVersions != null)
            {
                var latestVersion = doc.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion);
                if (latestVersion == null)
                {
                    return ResponseUtil.Error("Không tìm thấy phiên bản được khả thi",
                        ResponseMessages.FailedToSaveData,
                        HttpStatusCode.BadRequest);
                }
                
                if (workflow.Scope != Scope.InComing)
                {
                    var pathDoc = Path.Combine(Directory.GetCurrentDirectory(), "data","storage", "document",
                        documentId.ToString(), latestVersion.DocumentVersionId.ToString(), doc.DocumentName + ".pdf");
                    if (!File.Exists(pathDoc))
                    {
                        return ResponseUtil.Error("Không có file phù hợp", ResponseMessages.FailedToSaveData,
                            HttpStatusCode.BadRequest);
                    }

                    var docFile = await File.ReadAllBytesAsync(pathDoc);

                    var metadata = (_documentService.CheckMetaDataFile(pathDoc) ?? []).FindAll(x => x.IsValid).ToList();
                    var taskSign = doc.Tasks.FindAll(x => x.TaskType == TaskType.Sign);
                    
                    
                    // if (taskSign.Count != metadata.Count)
                    // {
                    //     return ResponseUtil.Error("Không đủ chữ ký, không thể lưu", ResponseMessages.FailedToSaveData,
                    //         HttpStatusCode.BadRequest);
                    // }

                    var archiveId = Guid.NewGuid();
                    if(doc.FinalArchiveDocumentId ==null)
                    {
                        archiveId = Guid.NewGuid();
                        var pathArchive = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage",
                            "archive_document", archiveId.ToString());
                        Directory.CreateDirectory(pathArchive);
                        await File.WriteAllBytesAsync(Path.Combine(pathArchive, doc.DocumentName + ".pdf"), docFile);
                        var signBys = taskSign.Select(x => x.User.UserName).ToList();
                        var signByString = $"[{string.Join(", ", signBys)}]";
                        var attachDoc = doc.AttachmentDocuments;
                        // List<AttachmentArchivedDocument> attachmentArchived =[];
                        foreach (var attachmentDocument in attachDoc)
                        {
                            var att = new AttachmentArchivedDocument()
                            {
                                //AttachmentArchivedDocumentId = archiveId,
                                AttachmentName = attachmentDocument.AttachmentDocumentName,
                                AttachmentUrl = attachmentDocument.AttachmentDocumentUrl,
                                ArchivedDocumentId = archiveId,
                            };
                            await _unitOfWork.AttachmentArchivedUOW.AddAsync(att);
                        }
                        var archiveDoc = new ArchivedDocument()
                        {
                            ArchivedDocumentId = archiveId,
                            ArchivedDocumentName = doc.DocumentName,
                            ArchivedDocumentContent = doc.DocumentContent,
                            SystemNumberOfDoc = doc.SystemNumberOfDoc,
                            NumberOfDocument = doc.NumberOfDocument,
                            SignedBy = signByString,
                            CreatedDate = DateTime.Now,
                            ExpirationDate = doc.ExpirationDate,
                            CreatedBy = currentTask.User.UserName,
                            ArchivedDocumentStatus = ArchivedDocumentStatus.Archived,
                            DateIssued = doc.DateIssued.Equals(DateTime.MaxValue) || doc.DateIssued.Equals(DateTime.MinValue) || doc.DateIssued == null ? DateTime.Now : doc.DateIssued,
                            Scope = (await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId)).Scope,
                            IsTemplate = false,
                            DocumentType = doc.DocumentType,
                            FinalDocumentId = doc.DocumentId,
                            ArchivedDocumentUrl = $"{_host}/api/Document/view-file/{archiveId}?version=1&isArchive=true"
                        };
                        doc.FinalArchiveDocumentId = archiveId;
                        doc.IsDeleted = true;
                        doc.ProcessingStatus = ProcessingStatus.Archived;
                        await _unitOfWork.ArchivedDocumentUOW.AddAsync(archiveDoc);
                    }
                    else
                    {
                        
                        archiveId = doc.FinalArchiveDocumentId??Guid.NewGuid();
                        var attachDoc = doc.AttachmentDocuments;
                        // List<AttachmentArchivedDocument> attachmentArchived =[];
                        foreach (var attachmentDocument in attachDoc)
                        {
                            var att = new AttachmentArchivedDocument()
                            {
                                //AttachmentArchivedDocumentId = archiveId,
                                AttachmentName = attachmentDocument.AttachmentDocumentName,
                                AttachmentUrl = attachmentDocument.AttachmentDocumentUrl,
                                ArchivedDocumentId = archiveId,
                            };
                            await _unitOfWork.AttachmentArchivedUOW.AddAsync(att);
                        }
                        var pathArchive = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage",
                            "archive_document", archiveId.ToString()!);
                        Directory.CreateDirectory(pathArchive);
                        await File.WriteAllBytesAsync(Path.Combine(pathArchive, doc.DocumentName + ".pdf"), docFile);
                        var signBys = taskSign.Where(x => x.User!=null).Select(x => x.User.UserName).ToList();
                        var signByString = $"[{string.Join(", ", signBys)}]";
                        var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(archiveId);
                        
                        if (archiveDoc == null)
                            return ResponseUtil.Error("Archive doc withdraw not found",
                                ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
                        archiveDoc.ArchivedDocumentName = doc.DocumentName;
                        archiveDoc.SystemNumberOfDoc = doc.SystemNumberOfDoc;
                        archiveDoc.ArchivedDocumentContent = doc.DocumentContent;
                        archiveDoc.NumberOfDocument = doc.NumberOfDocument;
                        archiveDoc.SignedBy = signByString;
                        archiveDoc.ExpirationDate = doc.ExpirationDate;
                        archiveDoc.ArchivedDocumentUrl =
                            $"{_host}/api/Document/view-file/{archiveId}?version=1&isArchive=true";
                        archiveDoc.CreatedBy = doc.User.UserName;
                        archiveDoc.ArchivedDocumentStatus = ArchivedDocumentStatus.Archived;
                        archiveDoc.Scope = (await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId)).Scope;
                        archiveDoc.DateIssued = doc.DateIssued.Equals(DateTime.MaxValue) || doc.DateIssued.Equals(DateTime.MinValue) || doc.DateIssued == null ? DateTime.Now : doc.DateIssued;
                        archiveDoc.DocumentType = doc.DocumentType;
                        archiveDoc.FinalDocumentId = doc.DocumentId;
                        
                        // doc.FinalArchiveDocumentId = archiveId;
                        doc.IsDeleted = true;
                        doc.ProcessingStatus = ProcessingStatus.Archived;
                        await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(archiveDoc);
                    }
                    
                    await _unitOfWork.DocumentUOW.UpdateAsync(doc);
                    var docSignList = latestVersion.DocumentSignatures;
                    foreach (var documentSignature in docSignList)
                    {
                        var tmp = new ArchiveDocumentSignature()
                        {
                            SignedAt = documentSignature.SignedAt,
                            OrderIndex = documentSignature.OrderIndex,
                            DigitalCertificateId = documentSignature.DigitalCertificateId,
                            ArchivedDocumentId = archiveId
                        };
                        await _unitOfWork.ArchiveDocumentSignatureUOW.AddAsync(tmp);
                    }

                    var distinctUserIds = orderedTasks
                        .Select(t => t.UserId)
                        .Distinct()
                        .ToList();

                    switch (workflow.Scope)
                    {
                        case Scope.School:
                        {
                            var liU = await _unitOfWork.UserUOW.FindAllUserAsync();
                            liU = liU.Where(x => x.IsDeleted == false && !x.Email.Contains("admin")).ToList();
                            distinctUserIds = liU.Select(x => x.UserId).ToList();
                            break;
                        }
                        case Scope.Division:
                        {
                            var listUser = orderedTasks.FirstOrDefault().User.Division.Users;
                            distinctUserIds = listUser.Select(x => x.UserId).ToList();
                            break;
                        }
                        case Scope.InComing:
                        case Scope.OutGoing:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    var userPermissions = new List<UserDocumentPermission>();

                    foreach (var userId in distinctUserIds)
                    {
                        var exists = await _unitOfWork.UserDocPermissionUOW.ExistsAsync(userId, archiveId);
                        if (!exists)
                        {
                            userPermissions.Add(new UserDocumentPermission
                            {
                                UserId = userId,
                                ArchivedDocumentId = archiveId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false,
                                GrantPermission = GrantPermission.Grant
                            });
                        }
                    }
                    
                    // Chỉ thêm những cái chưa có
                    if (userPermissions.Any())
                    {
                        await _unitOfWork.UserDocPermissionUOW.AddRangeAsync(userPermissions);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    
                    // await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    var archiveDoc = doc.FinalArchiveDocument;
                    var pathDoc = Path.Combine(Directory.GetCurrentDirectory(), "data","storage", "archive_document",
                        archiveDoc.ArchivedDocumentId.ToString(), doc.DocumentName + ".pdf");
                    if (!File.Exists(pathDoc))
                    {
                        return ResponseUtil.Error("Không có file phù hợp", ResponseMessages.FailedToSaveData,
                            HttpStatusCode.BadRequest);
                    }


                    var metadata = (_documentService.CheckMetaDataFile(pathDoc) ?? []).FindAll(x => x.IsValid).ToList();
                    // var taskSign = doc.Tasks.FindAll(x => x.TaskType == TaskType.Sign);
                    // if (taskSign.Count != metadata.Count)
                    // {
                    //     return ResponseUtil.Error("Không đủ chữ ký, không thể lưu", ResponseMessages.FailedToSaveData,
                    //         HttpStatusCode.BadRequest);
                    // }
                    var docSignList = latestVersion.DocumentSignatures;
                    var exitArchiveDocSign = archiveDoc.ArchiveDocumentSignatures;
                    // List<ArchiveDocumentSignature> archiveDocumentSignatures = new List<ArchiveDocumentSignature>();
                    foreach (var documentSignature in docSignList)
                    {
                        if(exitArchiveDocSign.Any(x => x.OrderIndex == documentSignature.OrderIndex)) continue;
                        var tmp = new ArchiveDocumentSignature()
                        {
                            SignedAt = documentSignature.SignedAt,
                            OrderIndex = documentSignature.OrderIndex,
                            DigitalCertificateId = documentSignature.DigitalCertificateId,
                            ArchivedDocumentId = archiveDoc.ArchivedDocumentId
                        };
                        await _unitOfWork.ArchiveDocumentSignatureUOW.AddAsync(tmp);
                    }
                    // var pathArchive = Path.Combine(Directory.GetCurrentDirectory(), "archive_document",
                    //     doc.DocumentName + ".pdf");
                    var signBys = metadata.Select(x => ExtractSigners(x.SignerName)).ToList();
                    var signByString = $"[{string.Join(", ", signBys)}]";
                    doc.IsDeleted = true;
                    archiveDoc.SignedBy = signByString;
                    await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(archiveDoc);
                    await _unitOfWork.DocumentUOW.UpdateAsync(doc);
                    // await _unitOfWork.SaveChangesAsync();
                }
            }
            else
            {
                return ResponseUtil.Error("Không tìm thấy phiên bản được khả thi", ResponseMessages.FailedToSaveData,
                    HttpStatusCode.BadRequest);
            }

            await _unitOfWork.SaveChangesAsync();

            // TODO: Gửi thông báo cho người tạo
            return ResponseUtil.GetObject("Tài liệu đã duyệt xong", ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        return ResponseUtil.Error("Không tìm thấy tài liệu", ResponseMessages.OperationFailed,
            HttpStatusCode.BadRequest);
    }


    private static string ExtractSigners(string? signature)
    {
        var regex = MyRegex();
        var result = regex.Match(signature ?? string.Empty);
        var extracted = result.Success ? result.Groups[1].Value : string.Empty;
        return extracted;
    }

    [GeneratedRegex(@"CN=([^,]+)")]
    private static partial Regex MyRegex();

    public async Task<ResponseDto> RejectDocumentActionAsync(RejectDocumentRequest rejectDocumentRequest)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(rejectDocumentRequest.UserId);
            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(rejectDocumentRequest.TaskId);

            if (task == null || task.UserId != rejectDocumentRequest.UserId)
                return ResponseUtil.Error(ResponseMessages.TaskNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (task.TaskStatus != TasksStatus.InProgress)
                return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var documentTask = task.Document;
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentTask!.DocumentId);
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var scope = document.DocumentWorkflowStatuses
                .FirstOrDefault()?.Workflow?.Scope ?? Scope.School;
            var orderedTasks = await GetOrderedTasks(document.Tasks,
                document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            var lastDocumentVersion = document.DocumentVersions
                .OrderByDescending(dv => dv.VersionNumber)
                .FirstOrDefault();
            var comment = new Comment
            {
                CommentContent = rejectDocumentRequest.Reason,
                CreateDate = DateTime.UtcNow,
                UserId = task.UserId,
                DocumentVersionId = lastDocumentVersion.DocumentVersionId,
                IsDeleted = false,
            };
            await _unitOfWork.CommentUOW.AddAsync(comment);
            
            var documentVersion =
                await _unitOfWork.DocumentVersionUOW.FindDocumentVersionByIdAsync(lastDocumentVersion
                    .DocumentVersionId);
            documentVersion.IsFinalVersion = false;
            await _unitOfWork.DocumentVersionUOW.UpdateAsync(documentVersion);
            await _unitOfWork.SaveChangesAsync();

            //if (scope != Scope.InComing)
            //{
                var documentVersion0 = document.DocumentVersions
                    .Where(d => d.VersionNumber.Equals("0"))
                    .FirstOrDefault();
                documentVersion0.IsFinalVersion = true;
                await _unitOfWork.DocumentVersionUOW.UpdateAsync(documentVersion0);
                
                await _unitOfWork.SaveChangesAsync();
            //}
                
                
            // 1. Cập nhật trạng thái task hiện tại
            task.TaskStatus = TasksStatus.Completed;
            task.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.TaskUOW.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

             //2. Cập nhật trạng thái tài liệu
            //var document = task.Document;
             //if (document == null)
                // return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
             document.ProcessingStatus = ProcessingStatus.Rejected;
             document.UpdatedDate = DateTime.UtcNow;

             //3. Hủy tất cả task còn lại (nếu chưa xử lý)
             var allPendingTasks = await _unitOfWork.TaskUOW.FindAllPendingTaskByDocumentIdAsync(task.DocumentId!.Value);
            
             foreach (var pendingTask in allPendingTasks)
             {
                 pendingTask.TaskStatus = TasksStatus.Revised; // Hoặc đặt là Rejected nếu bạn muốn thể hiện bị từ chối
                 pendingTask.UpdatedDate = DateTime.UtcNow;
             }

            var firstTask = orderedTasks[0].TaskId;
            foreach (var orderedTask in orderedTasks)
            {
                var userFinal = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTask.UserId);
                var notification = _notificationService.CreateDocRejectedNotification(task, orderedTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _notificationService.SendPushNotificationMobileAsync(userFinal.FcmToken, notification);
                await _hubContext.Clients.User(orderedTask.UserId.ToString())
                    .SendAsync("ReceiveMessage", notification);
                        
                        
                if (orderedTask.TaskId == firstTask)
                {
                    orderedTask.TaskStatus = TasksStatus.InProgress;
                    await _unitOfWork.TaskUOW.UpdateAsync(orderedTask);
                }
                else
                {
                    orderedTask.TaskStatus = TasksStatus.Revised;
                    orderedTask.StartDate = DateTime.MinValue;
                    orderedTask.EndDate = DateTime.MinValue;
                    orderedTask.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.TaskUOW.UpdateAsync(orderedTask);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            // 4. Gửi thông báo
            // TODO: Gửi thông báo cho người tạo tài liệu + người liên quan: "Tài liệu đã bị từ chối ở bước XYZ bởi User A"
            await transaction.CommitAsync();
            await _loggingService.WriteLogAsync(user.UserId,$"Văn bản với số hiệu {document.SystemNumberOfDoc} bị từ chối bởi: {user.FullName}");
            return ResponseUtil.GetObject(ResponseMessages.DocumentRejected, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
    }
}
