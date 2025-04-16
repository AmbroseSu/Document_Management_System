using System.Net;
using BusinessObject;
using DataAccess;
using DataAccess.DTO;
using MongoDB.Driver;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class NotificationService : INotificationService
{
    
    private readonly MongoDbService _mongoDbService;

    public NotificationService(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    public Notification CreateDocAcceptedNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = "Văn bản của bạn đã được chấp nhận",
            Content = $"Văn bản {task.Document?.DocumentName} đã được chấp nhận.",
            Type = "Document",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public Notification CreateDocRejectedNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Văn bản {task.Document?.DocumentName} đã bị từ chối bởi {task.User.FullName}",
            Content = $"Văn bản {task.Document?.DocumentName} đã bị từ chối bởi {task.User.FullName}",
            Type = "Document",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public Notification CreateNextUserDoTaskNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Văn bản {task.Document?.DocumentName} đã được chuyển đến cho bạn",
            Content = $"Task {task.Title} của văn bản {task.Document?.DocumentName} đã đến lượt của bạn.",
            Type = "Task",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public Notification CreateDocCompletedNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Văn bản {task.Document?.DocumentName} đã hoàn thành",
            Content = $"Văn bản {task.Document?.DocumentName} đã hoàn thành.",
            Type = "Document",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    

    public Notification CreateTaskAssignNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Task {task.Title} đã được giao cho bạn",
            Content = $"Task {task.Title} đã được giao cho bạn.",
            Type = "Task",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public Notification CreateTaskAcceptedNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Task {task.Title} của ${task.User.FullName} đã được chấp nhận",
            Content = $"Task {task.Title} của ${task.User.FullName} đã được chấp nhận.",
            Type = "Task",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public Notification CreateTaskRejectedNotification(Tasks task, Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Task {task.Title} của ${task.User.FullName} đã bị từ chối",
            Content = $"Task {task.Title} của ${task.User.FullName} đã bị từ chối",
            Type = "Task",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public async Task<ResponseDto> GetNotificationsByUserIdAsync(string userId, int page, int limit)
    {
        
        var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);

        var notifications = _mongoDbService.Notifications
            .Find(filter)
            .SortByDescending(n => n.CreatedAt);
        
        var totalRecords = notifications.Count();
        var totalPages = (int)Math.Ceiling((double)totalRecords / limit);

        IEnumerable<Notification> notificationResults = notifications
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToList();
        
        return ResponseUtil.GetCollection(notificationResults, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, (int)notifications.Count(), 1, 10, totalPages);;
    }
    
    public async Task MarkNotificationAsReadAsync(Guid notificationId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
    
        await _mongoDbService.Notifications.UpdateOneAsync(filter, update);
        
    }

}