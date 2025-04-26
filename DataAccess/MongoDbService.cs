using BusinessObject;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Task = System.Threading.Tasks.Task;

namespace DataAccess;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Notification> _notifications;
    private readonly IMongoCollection<Count> _counts;

    public MongoDbService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoSettings:ConnectionString"]);
        _database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
        _notifications = _database.GetCollection<Notification>("Notifications");
        _counts = _database.GetCollection<Count>("Counts");

    }
    
    public IMongoCollection<Notification> Notifications => _notifications;
    public IMongoCollection<Count> Counts => _counts;

    public async Task CreateNotificationAsync(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }
    
    public async Task CreateCountAsync(Count count)
    {
        await _counts.InsertOneAsync(count);
    }


}

