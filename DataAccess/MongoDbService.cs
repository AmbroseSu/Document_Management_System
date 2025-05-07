using BusinessObject;
using BusinessObject.Option;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Task = System.Threading.Tasks.Task;

namespace DataAccess;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Notification> _notifications;
    private readonly IMongoCollection<Count> _counts;
    private readonly IMongoCollection<LogEntry> _logs;


    public MongoDbService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoSettings:ConnectionString"]);
        _database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
        _notifications = _database.GetCollection<Notification>("Notifications");
        _counts = _database.GetCollection<Count>("Counts");
        _logs = _database.GetCollection<LogEntry>("logs");

    }
    
    public IMongoCollection<Notification> Notifications => _notifications;
    public IMongoCollection<Count> Counts => _counts;
    public IMongoCollection<LogEntry> Logs => _logs;

    public async Task CreateNotificationAsync(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }
    
    public async Task CreateCountAsync(Count count)
    {
        await _counts.InsertOneAsync(count);
    }
    
    public async Task CreateLogAsync(LogEntry log)
    {
        await _logs.InsertOneAsync(log);
    }


}

