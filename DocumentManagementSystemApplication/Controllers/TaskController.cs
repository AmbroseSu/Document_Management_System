using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Service;
using Service.SignalRHub;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }
        
        [HttpPost("create-task")]
        public async Task<ResponseDto> CreateTask([FromBody] TaskDto taskDto)
        {
            return await _taskService.CreateTask(taskDto);
        }
        
        [HttpPost("delete-task")]
        public async Task<ResponseDto> DeleteTask([FromQuery] Guid taskId)
        {
            return await _taskService.DeleteTaskAsync(taskId);
        }
        
        
        [HttpGet("view-documents-by-tab-for-user")]
        public async Task<ResponseDto> ViewDocumentsByTabForUser([FromQuery] Guid userId, [FromQuery] DocumentTab tab, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            return await _taskService.GetDocumentsByTabForUser(userId, tab, page, limit);
        }

        [HttpPost("create-handle-task-action")]
        public async Task<ResponseDto> HandleTaskAction([FromQuery] Guid taskId, [FromQuery] Guid userId,
            [FromQuery] TaskAction action)
        {
            return await _taskService.HandleTaskActionAsync(taskId, userId, action);
        }
        
        [HttpGet("view-all-tasks")]
        public async Task<ResponseDto> ViewAllTasks([FromQuery] Guid userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            return await _taskService.FindAllTasksAsync(userId, page, limit);
        }
        
        [HttpGet("view-task-by-id")]
        public async Task<ResponseDto> ViewTaskById([FromQuery] Guid id)
        {
            return await _taskService.FindTaskByIdAsync(id);
        }

    }
}
