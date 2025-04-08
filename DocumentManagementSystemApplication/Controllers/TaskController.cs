using BusinessObject;
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
        
    }
}
