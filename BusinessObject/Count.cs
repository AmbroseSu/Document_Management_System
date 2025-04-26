using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessObject;

public class Count
{
    [BsonId]
    [BsonRepresentation(BsonType.String)] // Lưu dưới dạng string
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("count")]
    public int Value { get; set; }

    [BsonElement("updateTime")]
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}