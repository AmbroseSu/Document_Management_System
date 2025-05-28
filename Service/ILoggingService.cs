using DataAccess.DTO;

namespace Service;

public interface ILoggingService
{
    Task WriteLogAsync(Guid userId, string message);
    Task<ResponseDto> GetAllLogsAsync(DateTime? startTime, DateTime? endTime,int page = 1, int pageSize = 10,string? query = null);
}