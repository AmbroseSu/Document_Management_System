using BusinessObject;
using DataAccess.DTO;

namespace Service;

public interface INotificationService
{
    Notification CreateDocAcceptedNotification(Tasks task, Guid userId);
    Notification CreateDocRejectedNotification(Tasks task, Guid userId);
    Notification CreateNextUserDoTaskNotification(Tasks task, Guid userId);
    Notification CreateDocCompletedNotification(Tasks task, Guid userId);
    Notification CreateTaskAssignNotification(Tasks task, Guid userId);
    Notification CreateTaskAcceptedNotification(Tasks task, Guid userId);
    Notification CreateTaskRejectedNotification(Tasks task, Guid userId);
    Task<ResponseDto> GetNotificationsByUserIdAsync(string userId, int page, int limit);
}