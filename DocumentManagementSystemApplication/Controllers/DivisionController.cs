using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DivisionController : ControllerBase
    {
        private readonly IDivisionService _divisionService;

        public DivisionController(IDivisionService divisionService)
        {
            _divisionService = divisionService;
        }
        
        [HttpPost("create-division")]
        [AuthorizeResource("[Division] Create Division")]
        public async Task<ResponseDto> CreateDivision([FromQuery] string divisionName)
        {
            return await _divisionService.AddDivisionAsync(divisionName);
        }
        
        [HttpPost("update-division")]
        [AuthorizeResource("[Division] Update Division")]
        public async Task<ResponseDto> UpdateDivision([FromBody] DivisionDto divisionDto)
        {
            return await _divisionService.UpdateDivisionAsync(divisionDto);
        }
        
        [HttpGet("view-all-division")]
        [AuthorizeResource("[Division] View All Division")]
        public async Task<ResponseDto> ViewAllDivision([FromQuery] string? divisionName, [FromQuery] int page = 1,[FromQuery] int limit = 10)
        {
            return await _divisionService.GetAllDivisionAsync(divisionName, page, limit);
        }
        
        [HttpPost("delete-division")]
        [AuthorizeResource("[Division] Delete Division")]
        public async Task<ResponseDto> DeleteDivision([FromQuery] Guid divisionId)
        {
            return await _divisionService.UpdateDivisionActiveOrDeleteAsync(divisionId);
        }
        
        [HttpGet("view-division-details")]
        [AuthorizeResource("[Division] View Division Details")]
        public async Task<ResponseDto> ViewDivisionDetails([FromQuery] Guid divisionId)
        {
            return await _divisionService.GetDivisionDetails(divisionId);
        }
    }
}
