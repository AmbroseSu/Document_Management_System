using System.Net;
using System.Net.Http.Headers;
using System.Text;
using BusinessObject;
using DataAccess;
using DataAccess.DTO;
using Google.Apis.Auth.OAuth2;
using MongoDB.Driver;
using Newtonsoft.Json;
using Service.Response;
using Service.Utilities;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            DocumentId = task.Document?.DocumentId.ToString(),
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
            DocumentId = task.Document?.DocumentId.ToString(),
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
            DocumentId = task.Document?.DocumentId.ToString(),
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
            DocumentId = task.Document?.DocumentId.ToString(),
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
            DocumentId = task.Document?.DocumentId.ToString(),
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
            DocumentId = task.Document?.DocumentId.ToString(),
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
            DocumentId = task.Document?.DocumentId.ToString(),
            WorkflowId = task.Document?.DocumentWorkflowStatuses?.FirstOrDefault()?.WorkflowId.ToString(),
            RedirectUrl = $"/task/{task.TaskId}", // frontend sẽ định nghĩa đường dẫn cụ thể
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
    
    public Notification TestNotification(Guid userId)
    {
        return new Notification
        {
            UserId = userId.ToString(),
            Title = $"Task Test của Test đã bị từ chối",
            Content = $"Task Test của Test đã bị từ chối",
            Type = "Task",
            TaskId = "fff74cdb-404b-46a5-b5c8-b79cb41d1a9f",
            DocumentId = "092abc80-61e9-46c3-84c4-91f8d4d19554",
            WorkflowId = "7dc95e1f-00c5-4791-9435-f7576d430712",
            RedirectUrl = $"/task/", // frontend sẽ định nghĩa đường dẫn cụ thể
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
    
    public async Task SendPushNotificationMobileAsync(string deviceToken, Notification notification)
    {
        var base64 = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIAL_JSON");
        var firebaseJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

        var credential = GoogleCredential
            .FromJson(firebaseJson)
            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

        // 3. Construct HTTP request to FCM v1
        var projectId = "dms-system-e835b"; // 🔺 Firebase project ID
        var url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";


        var message = new
        {
            message = new
            {
                token = deviceToken,
                notification = new
                {
                    title = notification.Title,
                    body = notification.Content
                },
                android = new
                {
                    priority = "high"
                },
                apns = new
                {
                    headers = new Dictionary<string, string>
                    {
                        { "apns-priority", "10" }
                    }
                },
                data = new Dictionary<string, string>
                {
                    { "notificationId", notification.Id.ToString() },
                    { "type", notification.Type ?? "" },
                    { "taskId", notification.TaskId ?? "" },
                    { "isRead", notification.IsRead.ToString() },
                    { "createAt", notification.CreatedAt.ToString() },
                    { "documentId", notification.DocumentId ?? "" },
                    { "workflowId", notification.WorkflowId ?? "" },
                    { "redirectUrl", notification.RedirectUrl ?? "" }
                }
            }
        };

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var content = new StringContent(JsonConvert.SerializeObject(message), System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"FCM Response: {response.StatusCode} - {responseBody}");
    }

}