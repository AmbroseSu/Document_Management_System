using BusinessObject;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Task = System.Threading.Tasks.Task;

namespace DataAccess;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Notification> _notifications;

    public MongoDbService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoSettings:ConnectionString"]);
        _database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
        _notifications = _database.GetCollection<Notification>("Notifications");
    }
    
    public IMongoCollection<Notification> Notifications => _notifications;

    public async Task CreateNotificationAsync(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }


}

