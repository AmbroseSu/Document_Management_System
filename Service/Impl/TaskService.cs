using System.Net;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using Microsoft.AspNetCore.SignalR;
using Repository;
using Service.Response;
using Service.SignalRHub;
using Service.Utilities;

namespace Service.Impl;

public class TaskService : ITaskService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    //private readonly IHubContext<NotificationHub> _hubContext;

    public TaskService(IMapper mapper, IUnitOfWork unitOfWork/*, IHubContext<NotificationHub> hubContext*/)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        //_hubContext = hubContext;
    }
    
    public async Task<ResponseDto> CreateTask(TaskDto taskDto)
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
                return ResponseUtil.Error(ResponseMessages.TaskStartdayEndDayFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            if (taskDto.EndDate <= taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
            var existingTasks = await _unitOfWork.TaskUOW.FindTaskByStepIdAsync(taskDto.StepId);
            var nextTaskNumber = existingTasks.Count() + 1;
            

            
            var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(taskDto.StepId);
            var currentFlow = currentStep!.FlowId;

            var workflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByFlowIdAsync(currentFlow);
            
            var workflowId = workflowFlow!.WorkflowId;
            
            var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var firstFlowInWorkflow = workflowFlowAll!.OrderBy(wf => wf.FlowNumber).FirstOrDefault();
            
            var stepAllOfFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(firstFlowInWorkflow!.FlowId);
            var firstStepInFlow = stepAllOfFlow!.OrderBy(s => s.StepNumber).FirstOrDefault();

            if (taskDto.StepId == firstStepInFlow!.StepId)
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
                taskDto.TaskStatus = TasksStatus.Pending;
            }
            taskDto.TaskNumber = nextTaskNumber;
            taskDto.CreatedDate = DateTime.Now;
            taskDto.IsDeleted = false;
            taskDto.IsActive = true;
            var task = new Tasks
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                StartDate = taskDto.StartDate,
                EndDate = taskDto.EndDate,
                TaskStatus = taskDto.TaskStatus ?? TasksStatus.Pending,
                TaskType = taskDto.TaskType,
                CreatedDate = DateTime.Now,
                TaskNumber = taskDto.TaskNumber,
                IsDeleted = taskDto.IsDeleted ?? false,
                IsActive = taskDto.IsActive ?? true,
                StepId = taskDto.StepId ?? Guid.Empty,
                DocumentId = taskDto.DocumentId ?? null,
                UserId = taskDto.UserId ?? Guid.Empty,
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
    
    
    public async Task<ResponseDto> GetDocumentsByTabForUser(Guid userId, DocumentTab tab, int page, int limit)
    {
    var allDocuments = await _unitOfWork.DocumentUOW.FindAllDocumentForTaskAsync(userId);

    var now = DateTime.UtcNow;
    
    IEnumerable<Document> filteredDocuments = new List<Document>();
    int totalRecords = 0;
    int totalPages = 0;
    IEnumerable<Document> documentResults = new List<Document>();
    IEnumerable<DivisionDto> result = new List<DivisionDto>();

    switch (tab)
    {
        case DocumentTab.All:
            filteredDocuments = allDocuments;
            break;

        case DocumentTab.Draft:
        {
            filteredDocuments = allDocuments
                .Where(d => d.UserId == userId &&
                            d.Tasks.All(t => t.TaskStatus == TasksStatus.Pending))
                .ToList();
            break;
        }

        case DocumentTab.Overdue:
        {
            filteredDocuments = allDocuments
                .Where(d => d.Tasks.Any(t => t.UserId == userId &&
                                             t.TaskStatus == TasksStatus.Pending &&
                                             d.Deadline < now))
                .ToList();
            break;
        }

        case DocumentTab.Rejected:
        {
            filteredDocuments = allDocuments
                .Where(d => d.Tasks.Any(t => t.UserId == userId &&
                                             t.TaskStatus == TasksStatus.Rejected))
                .ToList();
            break;
        }
            

        case DocumentTab.Accepted:
        {
            filteredDocuments = allDocuments
                .Where(d => d.Tasks.All(t => t.TaskStatus == TasksStatus.Completed))
                .ToList();
            break;
        }
            

        case DocumentTab.PendingApproval: // đến lượt duyệt
        {
            filteredDocuments = allDocuments
                .Where(d =>
                {
                    var myTask = d.Tasks.FirstOrDefault(t => t.UserId == userId);
                    if (myTask == null || myTask.TaskStatus != TasksStatus.Pending)
                        return false;

                    return d.Tasks
                        .Where(t => t.TaskNumber < myTask.TaskNumber)
                        .All(t => t.TaskStatus == TasksStatus.Completed);
                })
                .ToList();
            break;
        }

        case DocumentTab.Waiting:
        {
            filteredDocuments = allDocuments
                .Where(d =>
                {
                    var myTask = d.Tasks.FirstOrDefault(t => t.UserId == userId);
                    if (myTask == null || myTask.TaskStatus != TasksStatus.Completed)
                        return false;

                    return d.Tasks
                        .Where(t => t.TaskNumber > myTask.TaskNumber)
                        .Any(t => t.TaskStatus == TasksStatus.Pending);
                });
            break;
        }
    }
    
    totalRecords = filteredDocuments.Count();
    totalPages = (int)Math.Ceiling((double)totalRecords / limit);
    documentResults = filteredDocuments
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToList();

    result = _mapper.Map<IEnumerable<DivisionDto>>(documentResults);

    return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);
    
    }
    
    public async Task<ResponseDto> HandleTaskActionAsync(Guid taskId, Guid userId, TaskAction action)
    {
        var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(taskId);

    if (task == null || task.UserId != userId)
        return ResponseUtil.Error(ResponseMessages.TaskNotExists, ResponseMessages.OperationFailed,
            HttpStatusCode.NotFound);

    switch (action)
    {
        case TaskAction.AcceptTask:
        {
            task.TaskStatus = TasksStatus.Accepted;
            task.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            // Gửi thông báo cho người tạo: "Người A đã nhận xử lý"
            return ResponseUtil.GetObject(ResponseMessages.TaskHadAccepted, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        case TaskAction.RejectTask:
        {
            task.TaskStatus = TasksStatus.Rejected;
            task.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            // Gửi thông báo cho người tạo: "Người A từ chối xử lý"
            return ResponseUtil.GetObject(ResponseMessages.TaskHadRejected, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        case TaskAction.ApproveDocument:
        {
            if (task.TaskStatus != TasksStatus.Accepted)
                return ResponseUtil.Error(ResponseMessages.TaskHadNotAccepted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var tasksInSameStep = task.Step.Tasks.OrderBy(t => t.TaskNumber).ToList();
            var isMyTurn = tasksInSameStep
                .Where(t => t.TaskNumber < task.TaskNumber)
                .All(t => t.TaskStatus == TasksStatus.Completed);

            if (!isMyTurn)
                return ResponseUtil.Error(ResponseMessages.TaskHadNotCompleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            task.TaskStatus = TasksStatus.Completed;
            task.UpdatedDate = DateTime.UtcNow;
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
                return await ActivateNextTask(task);
        }
        
        case TaskAction.RejectDocument:
        {
            if (task.TaskStatus != TasksStatus.Accepted)
                return ResponseUtil.Error(ResponseMessages.TaskHadNotAccepted, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            // 1. Cập nhật trạng thái task hiện tại
            task.TaskStatus = TasksStatus.Rejected;
            task.UpdatedDate = DateTime.UtcNow;

            // 2. Cập nhật trạng thái tài liệu
            var document = task.Document;
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            document.ProcessingStatus = ProcessingStatus.Rejected;
            document.UpdatedDate = DateTime.UtcNow;

            // 3. Hủy tất cả task còn lại (nếu chưa xử lý)
            var allPendingTasks = await _unitOfWork.TaskUOW.FindAllPendingTaskByDocumentIdAsync(task.DocumentId!.Value);

            foreach (var pendingTask in allPendingTasks)
            {
                pendingTask.TaskStatus = TasksStatus.Rejected; // Hoặc đặt là Rejected nếu bạn muốn thể hiện bị từ chối
                pendingTask.UpdatedDate = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            // 4. Gửi thông báo
            // TODO: Gửi thông báo cho người tạo tài liệu + người liên quan: "Tài liệu đã bị từ chối ở bước XYZ bởi User A"

            return ResponseUtil.GetObject(ResponseMessages.DocumentRejected, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest, 1);
        }

    }
        return ResponseUtil.Error(ResponseMessages.InvalidAction, ResponseMessages.OperationFailed,
            HttpStatusCode.BadRequest);
    }
    
    
    private async Task<ResponseDto> ActivateNextTask(Tasks currentTask)
    {
        var currentStep = currentTask.Step;
        var currentFlow = currentStep.Flow;
        var documentId = currentTask.DocumentId;

        // 🔍 Tìm task kế tiếp trong cùng Step
        var tasksInStep = currentStep.Tasks.OrderBy(t => t.TaskNumber).ToList();
        var nextTask = tasksInStep.FirstOrDefault(t => t.TaskNumber > currentTask.TaskNumber);
        if (nextTask != null)
        {
            nextTask.TaskStatus = TasksStatus.InProgress;
            nextTask.UpdatedDate = DateTime.UtcNow;

            // TODO: Gửi thông báo đến nextTask.UserId
            await _unitOfWork.SaveChangesAsync();

            return ResponseUtil.GetObject("Đến lượt duyệt tiếp theo", ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        // Không còn task trong Step hiện tại — kiểm tra Step tiếp theo
        var stepsInFlow = currentFlow.Steps.OrderBy(s => s.StepNumber).ToList();
        var currentStepIndex = stepsInFlow.FindIndex(s => s.StepId == currentStep.StepId);

        if (currentStepIndex < stepsInFlow.Count - 1)
        {
            var nextStep = stepsInFlow[currentStepIndex + 1];
            var nextStepTasks = (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(nextStep.StepId, documentId.Value))
                .OrderBy(t => t.TaskNumber).ToList();
            var firstTaskInNextStep = nextStepTasks.FirstOrDefault();
            if (firstTaskInNextStep != null)
            {
                firstTaskInNextStep.TaskStatus = TasksStatus.InProgress;
                firstTaskInNextStep.UpdatedDate = DateTime.UtcNow;

                // TODO: Gửi thông báo
                await _unitOfWork.SaveChangesAsync();

                return ResponseUtil.GetObject("Chuyển sang bước tiếp theo", ResponseMessages.CreatedSuccessfully,
                    HttpStatusCode.OK, 1);
            }
        }
        
        var workflowFlow = await _unitOfWork.WorkflowFlowUOW
            .FindWorkflowFlowByFlowIdAsync(currentFlow.FlowId); // Hoặc FlowId thôi nếu đủ

        if (workflowFlow == null)
            return ResponseUtil.Error("Không tìm thấy WorkflowFlow", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

        var workflowId = workflowFlow.WorkflowId;
        
        // Step hiện tại là cuối cùng của Flow — kiểm tra Flow kế tiếp
        return await ActivateFirstTaskOfNextFlow(workflowId, currentFlow, documentId.Value);
    }
    
   private async Task<ResponseDto> ActivateFirstTaskOfNextFlow(Guid workflowId, Flow currentFlow, Guid documentId)
{
    // Lấy tất cả WorkflowFlow của Workflow hiện tại, theo thứ tự
    var workflowFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
    var orderedWorkflowFlows = workflowFlows.OrderBy(wf => wf.FlowNumber).ToList();

    var currentWorkflowFlowIndex = orderedWorkflowFlows.FindIndex(wf => wf.FlowId == currentFlow.FlowId);
    if (currentWorkflowFlowIndex < orderedWorkflowFlows.Count - 1)
    {
        var nextWorkflowFlow = orderedWorkflowFlows[currentWorkflowFlowIndex + 1];
        var nextFlow = nextWorkflowFlow.Flow;

        var nextSteps = nextFlow.Steps.OrderBy(s => s.StepNumber).ToList();
        var firstStep = nextSteps.FirstOrDefault();
        if (firstStep != null)
        {
            var firstTask = (await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(firstStep.StepId, documentId))
                            .OrderBy(t => t.TaskNumber)
                            .FirstOrDefault();
            if (firstTask != null)
            {
                firstTask.TaskStatus = TasksStatus.InProgress;
                firstTask.UpdatedDate = DateTime.UtcNow;

                // TODO: Gửi thông báo
                await _unitOfWork.SaveChangesAsync();

                return ResponseUtil.GetObject("Chuyển sang Flow tiếp theo", ResponseMessages.CreatedSuccessfully,
                    HttpStatusCode.OK, 1);
            }
        }
    }

    // Không còn Flow nào → đánh dấu document đã hoàn tất
    var doc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
    if (doc != null)
    {
        doc.ProcessingStatus = ProcessingStatus.Completed;
        doc.UpdatedDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // TODO: Gửi thông báo cho người tạo
        return ResponseUtil.GetObject("Tài liệu đã duyệt xong", ResponseMessages.CreatedSuccessfully,
            HttpStatusCode.OK, 1);
    }

    return ResponseUtil.Error("Không tìm thấy tài liệu", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
}
    
   
   /*private async Task<ResponseDto> PromoteToNextStepOrFlow(Step currentStep, Guid documentId)
    {
    var flow = currentStep.Flow;
    var allStepsInFlow = (await _unitOfWork.StepUOW.FindAllStepsInFlowAsync(flow.FlowId)).ToList();

    var currentStepIndex = allStepsInFlow.FindIndex(s => s.StepId == currentStep.StepId);
    if (currentStepIndex < allStepsInFlow.Count - 1)
    {
        var nextStep = allStepsInFlow[currentStepIndex + 1];

        var nextTasks = await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(nextStep.StepId, documentId);

        foreach (var t in nextTasks)
        {
            // TODO: Gửi thông báo cho t.UserId: "Đến lượt bạn duyệt"
        }

        return ResponseUtil.GetObject("Chuyển sang bước kế tiếp trong Flow", ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    var workflowFlows = flow.WorkflowFlows.OrderBy(wf => wf.FlowNumber).ToList();
    var currentFlowIndex = workflowFlows.FindIndex(wf => wf.FlowId == flow.FlowId);

    if (currentFlowIndex < workflowFlows.Count - 1)
    {
        var nextFlow = workflowFlows[currentFlowIndex + 1].Flow;

        var firstStepOfNextFlow = await _unitOfWork.StepUOW.GetFirstStepOfFlowAsync(nextFlow.FlowId);

        if (firstStepOfNextFlow != null)
        {
            var nextTasks = await _unitOfWork.TaskUOW.GetTasksByStepAndDocumentAsync(firstStepOfNextFlow.StepId, documentId);

            foreach (var t in nextTasks)
            {
                // TODO: Gửi thông báo cho t.UserId: "Đến lượt bạn duyệt"
            }
        }

        return ResponseUtil.GetObject("Chuyển sang Flow kế tiếp", ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
    if (document != null)
    {
        document.ProcessingStatus = ProcessingStatus.Completed;
        document.UpdatedDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // TODO: Gửi thông báo cho người tạo tài liệu: "Tài liệu đã duyệt xong"
        return ResponseUtil.GetObject(ResponseMessages.DocumentCompleted, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
    }*/

    
    
    
}