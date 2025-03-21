/*using BusinessObject;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Task = System.Threading.Tasks.Task;

namespace DataAccess;

public class MongoDbService
{
    private readonly IMongoCollection<Log> _logCollection;

    public MongoDbService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoSettings:ConnectionString"]);
        var database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
        _logCollection = database.GetCollection<Log>("Logs");
    }

    public async Task AddLogAsync(Log log)
    {
        await _logCollection.InsertOneAsync(log);
    }

    public async Task<List<Log>> GetLogsAsync()
    {
        return await _logCollection.Find(log => true).ToListAsync();
    }
}*/