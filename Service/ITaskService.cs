using DataAccess.DTO;

namespace Service;

public interface ITaskService
{
    Task<ResponseDto> CreateTask(TaskDto taskDto);
    Task<ResponseDto> DeleteTaskAsync(Guid id);
}