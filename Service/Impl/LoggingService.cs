using System.Net;
using BusinessObject.Option;
using DataAccess;
using DataAccess.DTO;
using MongoDB.Driver;
using Repository;
using Service.Utilities;

namespace Service.Impl;

public class LoggingService : ILoggingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly MongoDbService _mongo;

    public LoggingService(IUnitOfWork unitOfWork, MongoDbService mongo)
    {
        _unitOfWork = unitOfWork;
        _mongo = mongo;
    }

    public async Task WriteLogAsync(Guid userId, string message)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        if (user?.UserName == null)
        {
            return;
        }
        var log = new LogEntry()
        {
            UserName = user.UserName,
            Action = message,
            Timestamp = DateTime.UtcNow
        };
        await _mongo.CreateLogAsync(log);
    }

    public async Task<ResponseDto> GetAllLogsAsync()
    {
        var logs = _mongo.Logs.AsQueryable()
            .Select(log => new LogEntry
            {
                Timestamp = log.Timestamp,
                UserName = log.UserName,
                Action = log.Action
            }).ToList();
        return ResponseUtil.GetCollection(logs, "Logs", HttpStatusCode.Accepted, 10, 1, 10, 10);
    }
}