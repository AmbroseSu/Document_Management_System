using BusinessObject.Enums;
using DataAccess.DTO;

namespace Service;

public interface ITaskService
{
    Task<ResponseDto> CreateTask(TaskDto taskDto);
    Task<ResponseDto> DeleteTaskAsync(Guid id);
    Task<ResponseDto> GetDocumentsByTabForUser(Guid userId, DocumentTab tab, int page, int limit);
    Task<ResponseDto> HandleTaskActionAsync(Guid taskId, Guid userId, TaskAction action);
    Task<ResponseDto> FindAllTasksAsync(Guid userId, int page, int limit);
    Task<ResponseDto> FindTaskByIdAsync(Guid id);
}