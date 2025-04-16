using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface ITaskService
{
    Task<ResponseDto> CreateTask(TaskDto taskDto);
    Task<ResponseDto> DeleteTaskAsync(Guid id);
    Task<ResponseDto> UpdateTaskAsync(TaskRequest taskRequest);
    Task<ResponseDto> FindAllTaskByDocumentIdAsync(Guid documentId);
    Task<ResponseDto> GetDocumentsByTabForUser(Guid userId, DocumentTab tab, int page, int limit);
    Task<ResponseDto> HandleTaskActionAsync(Guid taskId, Guid userId, TaskAction action);
    Task<ResponseDto> FindAllTasksAsync(Guid userId, int page, int limit);
    Task<ResponseDto> FindTaskByIdAsync(Guid id);
}