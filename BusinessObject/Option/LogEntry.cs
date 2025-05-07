namespace BusinessObject.Option;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; }
}