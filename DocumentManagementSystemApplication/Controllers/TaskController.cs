using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }
        
        [HttpPost("create-task")]
        //[AuthorizeResource("[Task] Create Task")]
        public async Task<ResponseDto> CreateTask([FromBody] TaskDto taskDto)
        {
            var id = User.FindFirst("userid")?.Value;
            return await _taskService.CreateTask(Guid.Parse(id),taskDto);
        }
        
        [HttpPost("create-first-task")]
        //[AuthorizeResource("[Task] Create First Task")]
        public async Task<ResponseDto> CreateFirstTask([FromBody] TaskDto taskDto)
        {
            return await _taskService.CreateFirstTask(taskDto);
        }
        
        [HttpPost("delete-task")]
        //[AuthorizeResource("[Task] Delete Task")]
        public async Task<ResponseDto> DeleteTask([FromQuery] Guid taskId)
        {
            return await _taskService.DeleteTaskAsync(taskId);
        }
        
        [HttpPost("update-task")]
        //[AuthorizeResource("[Task] Update Task")]
        public async Task<ResponseDto> UpdateTask([FromBody] TaskRequest taskRequest)
        {
            return await _taskService.UpdateTaskAsync(taskRequest);
        }
        
        [HttpGet("view-all-tasks-by-document-id")]
        //[AuthorizeResource("[Task] View All Tasks By Document Id")]
        public async Task<ResponseDto> ViewAllTasksByDocumentId([FromQuery] Guid documentId)
        {
            return await _taskService.FindAllTaskByDocumentIdAsync(documentId);
        }
        
        
        [HttpGet("view-documents-by-tab-for-user")]
        //[AuthorizeResource("[Task] View Documents By Tab For User")]
        public async Task<ResponseDto> ViewDocumentsByTabForUser([FromQuery] String? docName, [FromQuery] Scope? scope, [FromQuery] Guid userId, [FromQuery] DocumentTab tab, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            return await _taskService.GetDocumentsByTabForUser(docName, scope, userId, tab, page, limit);
        }

        [HttpPost("create-handle-task-action")]
        //[AuthorizeResource("[Task] Create Handle Task Action")]
        public async Task<ResponseDto> HandleTaskAction([FromQuery] Guid taskId, [FromQuery] Guid userId,
            [FromQuery] TaskAction action)
        {
            return await _taskService.HandleTaskActionAsync(taskId, userId, action);
        }
        
        [HttpGet("view-all-tasks")]
        //[AuthorizeResource("[Task] View All Tasks")]
        public async Task<ResponseDto> ViewAllTasks([FromQuery] Guid userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            return await _taskService.FindAllTasksAsync(userId, page, limit);
        }
        
        [HttpGet("view-task-by-id")]
        //[AuthorizeResource("[Task] View Task By Id")]
        public async Task<ResponseDto> ViewTaskById([FromQuery] Guid id)
        {
            return await _taskService.FindTaskByIdAsync(id);
        }
        
        [HttpPost("create-reject-document-action")]
        //[AuthorizeResource("[Task] Create Reject Document Action")]
        public async Task<ResponseDto> RejectDocumentAction([FromBody] RejectDocumentRequest rejectDocumentRequest)
        {
            return await _taskService.RejectDocumentActionAsync(rejectDocumentRequest);
        }

    }
}
