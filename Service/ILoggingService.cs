using DataAccess.DTO;

namespace Service;

public interface ILoggingService
{
    Task WriteLogAsync(Guid userId, string message);
    Task<ResponseDto> GetAllLogsAsync(int page = 1, int pageSize = 10);
}