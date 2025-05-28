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

    public async Task<ResponseDto> GetAllLogsAsync(DateTime? startTime, DateTime? endTime,int page = 1, int pageSize = 10,string? query = null)
    {
        
        var logs = _mongo.Logs.AsQueryable()
            .Select(log => new LogEntry
            {
                Timestamp = log.Timestamp,
                UserName = log.UserName,
                Action = log.Action
            }).ToList();
        if (!string.IsNullOrEmpty(query))
        {
            logs = logs.Where(log => log.UserName.Contains(query, StringComparison.OrdinalIgnoreCase) || log.Action.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // startTime = startTime?;
        // endTime = endTime?.AddHours(23).AddMinutes(59).AddSeconds(59);
        if (startTime != null)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            logs = logs.Where(log => TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, timeZone).CompareTo(startTime) >= 0).ToList();
        }
        if (endTime != null)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            logs = logs.Where(log => TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, timeZone).CompareTo(endTime) <= 0).ToList();
        }
        var totalPage = logs.Count / pageSize + (logs.Count % pageSize > 0 ? 1 : 0);
        return ResponseUtil.GetCollection(logs.Skip((page - 1) * pageSize).Take(pageSize).ToList(), "Logs", HttpStatusCode.Accepted, logs.Count, page, pageSize, totalPage);
    }
}