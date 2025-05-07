using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessObject.Option;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Timestamp { get; set; }
    [BsonElement("userName")]
    public string UserName { get; set; }
    [BsonElement("action")]
    public string Action { get; set; }
}