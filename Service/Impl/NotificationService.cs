using BusinessObject;

namespace Service.Impl;

public class NotificationService
{
    public Notification CreateTaskAcceptedNotification(Tasks task, string userId)
    {
        return new Notification
        {
            UserId = userId,
            Title = "Văn bản của bạn đã được chấp nhận",
            Content = $"Văn bản {task.Document?.DocumentName} đã được chấp nhận.",
            Type = "document",
            TaskId = task.TaskId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
}