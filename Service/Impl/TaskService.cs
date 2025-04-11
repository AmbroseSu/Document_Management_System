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
    
    
    /*public async Task<ResponseDto> GetDocumentsByTabForUser(Guid userId, DocumentTab tab, int page, int limit)
{
    var allDocuments = await _unitOfWork.DocumentUOW.FindAllDocumentForTaskAsync(userId);

    var now = DateTime.UtcNow;

    switch (tab)
    {
        case DocumentTab.All:
            var totalRecords = allDocuments.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / limit);
            IEnumerable<Document> documentResults = allDocuments
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            var result = _mapper.Map<IEnumerable<DivisionDto>>(documentResults);
            return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);

        case DocumentTab.Draft:
            return allDocuments
                .Where(d => d.UserId == userId &&
                            d.Tasks.All(t => t.TaskStatus == TasksStatus.Pending))
                .ToList();

        case DocumentTab.Overdue:
            return allDocuments
                .Where(d => d.Tasks.Any(t => t.UserId == userId &&
                                             t.TaskStatus == TasksStatus.Pending &&
                                             d.Deadline < now))
                .ToList();

        case DocumentTab.Rejected:
            return allDocuments
                .Where(d => d.Tasks.Any(t => t.UserId == userId &&
                                             t.TaskStatus == TasksStatus.Rejected))
                .ToList();

        case DocumentTab.Accepted:
            return allDocuments
                .Where(d => d.Tasks.All(t => t.TaskStatus == TasksStatus.Done))
                .ToList();

        case DocumentTab.PendingApproval: // đến lượt duyệt
            return allDocuments
                .Where(d =>
                {
                    var myTask = d.Tasks.FirstOrDefault(t => t.UserId == userId);
                    if (myTask == null || myTask.TaskStatus != TasksStatus.Pending)
                        return false;

                    return d.Tasks
                             .Where(t => t.TaskNumber < myTask.TaskNumber)
                             .All(t => t.TaskStatus == TasksStatus.Done);
                })
                .ToList();

        case DocumentTab.Waiting: // đang chờ duyệt
            return allDocuments
                .Where(d =>
                {
                    var myTask = d.Tasks.FirstOrDefault(t => t.UserId == userId);
                    if (myTask == null || myTask.TaskStatus != TasksStatus.Done)
                        return false;

                    return d.Tasks
                             .Where(t => t.TaskNumber > myTask.TaskNumber)
                             .Any(t => t.TaskStatus == TasksStatus.Pending);
                })
                .ToList();

        default:
            return new List<Document>();
    }
}
*/

    
    
    
    
    
}