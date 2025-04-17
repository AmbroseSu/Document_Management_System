using System.Net;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Repository;
using Service.Response;
using Service.SignalRHub;
using Service.Utilities;

namespace Service.Impl;

public class TaskService : ITaskService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly MongoDbService _notificationCollection;
    private readonly IHubContext<NotificationHub> _hubContext;

    public TaskService(IMapper mapper, IUnitOfWork unitOfWork, INotificationService notificationService, MongoDbService notificationCollection , IHubContext<NotificationHub> hubContext)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _notificationCollection = notificationCollection;
        _hubContext = hubContext;
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

            if (taskDto.EndDate >= taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
            var existingTasks = await _unitOfWork.TaskUOW.FindTaskByStepIdDocIdAsync(taskDto.StepId, taskDto.DocumentId);
            var nextTaskNumber = existingTasks.Count() + 1;
            

            
            var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(taskDto.StepId);
            var currentFlow = currentStep!.FlowId;

            var workflowFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByFlowIdAsync(currentFlow);
            
            var workflowId = workflowFlow!.WorkflowId;
            
            var workflowFlowAll = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var firstFlowInWorkflow = workflowFlowAll!.OrderBy(wf => wf.FlowNumber).FirstOrDefault();
            
            var stepAllOfFlow = await _unitOfWork.StepUOW.FindStepByFlowIdAsync(firstFlowInWorkflow!.FlowId);
            var firstStepInFlow = stepAllOfFlow!.OrderBy(s => s.StepNumber).FirstOrDefault();

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
            var orderedTasks = await GetOrderedTasks(document.Tasks, document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            if(orderedTasks[orderedTasks.Count - 1].EndDate > taskDto.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayLowerEndDaypreviousStepFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            
            if (taskDto.StepId == firstStepInFlow!.StepId)
            {
                taskDto.TaskStatus = TasksStatus.Pending;
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
            taskDto.TaskId = Guid.NewGuid();
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

    public async Task<ResponseDto> UpdateTaskAsync(TaskRequest taskRequest)
    {
        try
        {
            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(taskRequest.TaskId);
            if (task == null)
                return ResponseUtil.Error(ResponseMessages.TaskNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (task.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.TaskAlreadyDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (taskRequest.StartDate < DateTime.Now || taskRequest.EndDate < DateTime.Now)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (taskRequest.EndDate >= taskRequest.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskEndDayFailed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(task.DocumentId);
            var orderedTasks = await GetOrderedTasks(document.Tasks, document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
            if(orderedTasks[orderedTasks.Count - 1].EndDate > taskRequest.StartDate)
                return ResponseUtil.Error(ResponseMessages.TaskStartdayLowerEndDaypreviousStepFailed, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            var hasChanges = false;
            
            if (!string.IsNullOrWhiteSpace(taskRequest.Title) && !task.Title.Equals(taskRequest.Title))
            {
                task.Title = taskRequest.Title;
                hasChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(taskRequest.Description) && !task.Title.Equals(taskRequest.Description))
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
                var user = await _unitOfWork.UserUOW.FindUserByIdAsync(taskRequest.UserId.Value);
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
            
                var currentStep = await _unitOfWork.StepUOW.FindStepByIdAsync(task.StepId);
                var matchedRoles = primaryRoles.Where(role => role.Equals(currentStep.Role.RoleName.ToLower())).ToList();
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
           
           foreach (var task in tasks)
           {
               var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(task.DocumentId!.Value);
               if (document == null)
                   return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                       HttpStatusCode.NotFound);
               var orderedTasks = await GetOrderedTasks(document.Tasks, document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
               var taskDto = _mapper.Map<TaskDto>(task);
               var taskDetail = new TaskDetail();
               taskDetail.TaskDto = taskDto;
               taskDetail.Scope = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope;
               taskDetail.WorkflowName = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowName;
               taskDetail.StepAction = task.Step.Action;
               taskDetail.DocumentTypeName = task.Document.DocumentType.DocumentTypeName;
               var user = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTasks.First().UserId);
                taskDetail.UserNameCreateTask = user.FullName;
               taskDetails.Add(taskDetail);
           }
           
           
           var totalRecords = tasks.Count();
           var totalPages = (int)Math.Ceiling((double)totalRecords / limit);

           IEnumerable<TaskDetail> tasksResults = taskDetails
               .Skip((page - 1) * limit)
               .Take(limit)
               .ToList();
           
           // var result = _mapper.Map<IEnumerable<TaskDto>>(tasksResults);
           return ResponseUtil.GetCollection(tasksResults, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, tasks.Count(), 1, 10, totalPages);
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
              var orderedTasks = await GetOrderedTasks(document.Tasks, document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
              
           var taskDto = _mapper.Map<TaskDto>(task);
           var result = new TaskDetail();
           result.TaskDto = taskDto;
           result.Scope = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope;
           result.WorkflowName = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowName;
           result.WorkflowId = task.Document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowId;
           result.StepAction = task.Step.Action;
           result.DocumentId = document.DocumentId;
           result.DocumentTypeName = task.Document.DocumentType.DocumentTypeName;
           var user = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTasks.First().UserId);
           result.UserNameCreateTask = user.FullName;
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
   
   
    
    
    public async Task<ResponseDto> GetDocumentsByTabForUser(Guid userId, DocumentTab tab, int page, int limit)
    {
        try
        {
            var allDocuments = await _unitOfWork.DocumentUOW.FindAllDocumentForTaskAsync(userId);

    var now = DateTime.UtcNow;
    
    IEnumerable<Document> filteredDocuments = new List<Document>();
    int totalRecords = 0;
    int totalPages = 0;
    IEnumerable<Document> documentResults = new List<Document>();
    IEnumerable<DocumentDto> result = new List<DocumentDto>();

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
                                             t.TaskStatus == TasksStatus.Revised))
                .ToList();
            break;
            
            IEnumerable<Document> documentRejectResponses = new List<Document>();
            foreach (var allDocument in allDocuments)
            {
                var documentVersion = await _unitOfWork.DocumentVersionUOW.FindDocumentVersionByDocumentIdAsync(allDocument.DocumentId);
                
            }
            // docId, docName, docType, versionId, versionNumber, date reject, user reject
        }
            

        case DocumentTab.Accepted:
        {
            var acceptedDocuments = new List<Document>();

            foreach (var doc in allDocuments)
            {
                var orderedTasks = await GetOrderedTasks(
                    doc.Tasks,
                    doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty
                );

                for (int i = 0; i < orderedTasks.Count - 1; i++)
                {
                    var currentTask = orderedTasks[i];
                    var nextTask = orderedTasks[i + 1];

                    if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                    {
                        if (nextTask.TaskStatus == TasksStatus.Completed)
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
                    doc.Tasks,
                    doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty
                );

                for (int i = 0; i < orderedTasks.Count; i++)
                {
                    var currentTask = orderedTasks[i];

                    if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.InProgress)
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
                var orderedTasks = await GetOrderedTasks(doc.Tasks, doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);

                for (int i = 0; i < orderedTasks.Count - 1; i++)
                {
                    var currentTask = orderedTasks[i];
                    var nextTask = orderedTasks[i + 1];

                    if (currentTask.UserId == userId && currentTask.TaskStatus == TasksStatus.Completed)
                    {
                        if (nextTask.TaskStatus == TasksStatus.InProgress && nextTask.UserId != userId)
                        {
                            waitingDocuments.Add(doc);
                            break; // Thỏa điều kiện => thêm document và thoát vòng lặp
                        }
                    }
                }
            }

            filteredDocuments = waitingDocuments;
            break;
        }
    }
    
    totalRecords = filteredDocuments.Count();
    totalPages = (int)Math.Ceiling((double)totalRecords / limit);
    documentResults = filteredDocuments
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToList();

    result = _mapper.Map<IEnumerable<DocumentDto>>(documentResults);

    return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);
    
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
                var wfFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAndFlowIdAsync(workflowId, flowId);
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
            Console.WriteLine($"TaskId: {x.TaskId}, FlowNumber: {flowNumber}, StepNumber: {x.Step?.StepNumber}, TaskNumber: {x.TaskNumber}");
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
    var orderedTasks = await GetOrderedTasks(document.Tasks, document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
    var user = await _unitOfWork.UserUOW.FindUserByIdAsync(orderedTasks.First().UserId);
    
    
    switch (action)
    {
        case TaskAction.AcceptTask:
        {
            task.TaskStatus = TasksStatus.Waiting;
            task.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            
            var notification = _notificationService.CreateTaskAcceptedNotification(task, user.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
            
            await transaction.CommitAsync();
            // Gửi thông báo cho người tạo: "Người A đã nhận xử lý"
            return ResponseUtil.GetObject(ResponseMessages.TaskHadAccepted, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        case TaskAction.RejectTask:
        {
            task.TaskStatus = TasksStatus.Revised;
            task.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            
            var notification = _notificationService.CreateTaskRejectedNotification(task, user.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);

            await transaction.CommitAsync();
            // Gửi thông báo cho người tạo: "Người A từ chối xử lý"
            return ResponseUtil.GetObject(ResponseMessages.TaskHadRejected, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.OK, 1);
        }

        case TaskAction.ApproveDocument:
        {
            if (task.TaskStatus != TasksStatus.InProgress)
                return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed,
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
                var result = await ActivateNextTask(task);
                await transaction.CommitAsync();
                return result;
        }
        
        /*case TaskAction.RejectDocument:
        {
            if (task.TaskStatus != TasksStatus.InProgress)
                return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            // 1. Cập nhật trạng thái task hiện tại
            task.TaskStatus = TasksStatus.Revised;
            task.UpdatedDate = DateTime.UtcNow;

            // 2. Cập nhật trạng thái tài liệu
            //var document = task.Document;
            if (document == null)
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            document.ProcessingStatus = ProcessingStatus.Rejected;
            document.UpdatedDate = DateTime.UtcNow;

            // 3. Hủy tất cả task còn lại (nếu chưa xử lý)
            var allPendingTasks = await _unitOfWork.TaskUOW.FindAllPendingTaskByDocumentIdAsync(task.DocumentId!.Value);

            foreach (var pendingTask in allPendingTasks)
            {
                pendingTask.TaskStatus = TasksStatus.Revised; // Hoặc đặt là Rejected nếu bạn muốn thể hiện bị từ chối
                pendingTask.UpdatedDate = DateTime.UtcNow;
            }

            foreach (var orderedTask in orderedTasks)
            {
                var notification = _notificationService.CreateDocRejectedNotification(task, orderedTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(orderedTask.UserId.ToString()).SendAsync("ReceiveMessage", notification);

            }

            await _unitOfWork.SaveChangesAsync();
            // 4. Gửi thông báo
            // TODO: Gửi thông báo cho người tạo tài liệu + người liên quan: "Tài liệu đã bị từ chối ở bước XYZ bởi User A"
            await transaction.CommitAsync();
            return ResponseUtil.GetObject(ResponseMessages.DocumentRejected, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest, 1);
        }*/

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
        var tasksInStep = stepp.Tasks.OrderBy(t => t.TaskNumber).ToList();
        var nextTask = tasksInStep.FirstOrDefault(t => t.TaskNumber > currentTask.TaskNumber);
        var previousTask = tasksInStep
            .Where(t => t.TaskNumber < currentTask.TaskNumber)
            .OrderByDescending(t => t.TaskNumber)
            .FirstOrDefault();
        
        if (previousTask != null)
        {
            var notification = _notificationService.CreateDocAcceptedNotification(previousTask, previousTask.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
        }
        if (nextTask != null)
        {
            nextTask.TaskStatus = TasksStatus.InProgress;
            nextTask.UpdatedDate = DateTime.UtcNow;

            // TODO: Gửi thông báo đến nextTask.UserId
            
            var notification = _notificationService.CreateNextUserDoTaskNotification(nextTask, nextTask.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);

            await _unitOfWork.SaveChangesAsync();

            return ResponseUtil.GetObject($"Đến lượt duyệt tiếp theo:{nextTask.UserId}", ResponseMessages.CreatedSuccessfully,
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
            var finalTaskInCurrentStep = tasksInStep.OrderByDescending(t => t.TaskNumber).FirstOrDefault();
            if (finalTaskInCurrentStep != null)
            {
                var notification = _notificationService.CreateDocAcceptedNotification(finalTaskInCurrentStep, finalTaskInCurrentStep.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
            }
            if (firstTaskInNextStep != null)
            {
                firstTaskInNextStep.TaskStatus = TasksStatus.InProgress;
                firstTaskInNextStep.UpdatedDate = DateTime.UtcNow;

                // TODO: Gửi thông báo
                var notification = _notificationService.CreateNextUserDoTaskNotification(firstTaskInNextStep, firstTaskInNextStep.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
                await _unitOfWork.SaveChangesAsync();

                return ResponseUtil.GetObject($"Chuyển sang bước tiếp theo: {firstTaskInNextStep.UserId}", ResponseMessages.CreatedSuccessfully,
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
    var task = new Tasks();
    var currentWorkflowFlowIndex = orderedWorkflowFlows.FindIndex(wf => wf.FlowId == currentFlow.FlowId);
    var currentWorkflowFlow = orderedWorkflowFlows.Where(wf => wf.FlowId == currentFlow.FlowId ).FirstOrDefault();
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
                var notification = _notificationService.CreateDocAcceptedNotification(finalTask, finalTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
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
                    
                    var nextDocumentWorkflowflowStatus = new DocumentWorkflowStatus{ 
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
                var notification = _notificationService.CreateNextUserDoTaskNotification(firstTask, firstTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveMessage", notification);
                
                return ResponseUtil.GetObject($"Chuyển sang Flow tiếp theo: {firstTask.UserId}", ResponseMessages.CreatedSuccessfully,
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
        
        var documentVersions = await _unitOfWork.DocumentVersionUOW.FindDocumentVersionByDocumentIdAsync(documentId);
        if (documentVersions != null)
        {
            var documentVersion = documentVersions.OrderByDescending(dv => dv.VersionNumber).FirstOrDefault();
            documentVersion.IsFinalVersion = true;
            await _unitOfWork.DocumentVersionUOW.UpdateAsync(documentVersion);
            await _unitOfWork.SaveChangesAsync();
        }
        
        var orderedTasks = await GetOrderedTasks(doc.Tasks, doc.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);

        foreach (var orderedTask in orderedTasks)
        {
            var notification = _notificationService.CreateDocCompletedNotification(task, task.UserId);
            await _notificationCollection.CreateNotificationAsync(notification);
            await _hubContext.Clients.User(orderedTask.UserId.ToString()).SendAsync("ReceiveMessage", notification);

        }
        
        await _unitOfWork.SaveChangesAsync();

        // TODO: Gửi thông báo cho người tạo
        return ResponseUtil.GetObject("Tài liệu đã duyệt xong", ResponseMessages.CreatedSuccessfully,
            HttpStatusCode.OK, 1);
    }

    return ResponseUtil.Error("Không tìm thấy tài liệu", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
}
   
   
   
    
   public async Task<ResponseDto> RejectDocumentActionAsync(RejectDocumentRequest rejectDocumentRequest)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var task = await _unitOfWork.TaskUOW.FindTaskByIdAsync(rejectDocumentRequest.TaskId);

        if (task == null || task.UserId != rejectDocumentRequest.UserId)
             return ResponseUtil.Error(ResponseMessages.TaskNotExists, ResponseMessages.OperationFailed,
              HttpStatusCode.NotFound);
        if (task.TaskStatus != TasksStatus.InProgress)
            return ResponseUtil.Error(ResponseMessages.TaskHadNotYourTurn, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

        var documentTask = task.Document;
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentTask!.DocumentId);
        if (document == null)
              return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
            HttpStatusCode.NotFound);
        var orderedTasks = await GetOrderedTasks(document.Tasks, document.DocumentWorkflowStatuses.FirstOrDefault()?.WorkflowId ?? Guid.Empty);
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
        var documentVersion = await _unitOfWork.DocumentVersionUOW.FindDocumentVersionByIdAsync(lastDocumentVersion.DocumentVersionId);
        documentVersion.IsFinalVersion = false;
        await _unitOfWork.DocumentVersionUOW.UpdateAsync(documentVersion);
        await _unitOfWork.SaveChangesAsync();
            

            // 1. Cập nhật trạng thái task hiện tại
            task.TaskStatus = TasksStatus.Completed;
            task.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.TaskUOW.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // 2. Cập nhật trạng thái tài liệu
            //var document = task.Document;
            // if (document == null)
            //     return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            //
            // document.ProcessingStatus = ProcessingStatus.Rejected;
            // document.UpdatedDate = DateTime.UtcNow;

            // 3. Hủy tất cả task còn lại (nếu chưa xử lý)
            var allPendingTasks = await _unitOfWork.TaskUOW.FindAllPendingTaskByDocumentIdAsync(task.DocumentId!.Value);

            foreach (var pendingTask in allPendingTasks)
            {
                pendingTask.TaskStatus = TasksStatus.Revised; // Hoặc đặt là Rejected nếu bạn muốn thể hiện bị từ chối
                pendingTask.UpdatedDate = DateTime.UtcNow;
            }

            foreach (var orderedTask in orderedTasks)
            {
                var notification = _notificationService.CreateDocRejectedNotification(task, orderedTask.UserId);
                await _notificationCollection.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(orderedTask.UserId.ToString()).SendAsync("ReceiveMessage", notification);

            }

            await _unitOfWork.SaveChangesAsync();
            // 4. Gửi thông báo
            // TODO: Gửi thông báo cho người tạo tài liệu + người liên quan: "Tài liệu đã bị từ chối ở bước XYZ bởi User A"
            await transaction.CommitAsync();
            return ResponseUtil.GetObject(ResponseMessages.DocumentRejected, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest, 1);
            
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
    }
   

   
    
    
}