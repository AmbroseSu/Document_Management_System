using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessObject;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.String)] // Lưu dưới dạng GUID string
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("content")]
    public string Content { get; set; }

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("type")]
    public string Type { get; set; } // Ví dụ: "task", "workflow", "approve"

    [BsonElement("taskId")]
    [BsonIgnoreIfNull]
    public string? TaskId { get; set; }

    [BsonElement("workflowId")]
    [BsonIgnoreIfNull]
    public string? WorkflowId { get; set; }

    [BsonElement("redirectUrl")]
    [BsonIgnoreIfNull]
    public string? RedirectUrl { get; set; }
}