using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResourceController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpPost("create-resource")]
    //AuthorizeResource("[Resource] Create Resource")]
    public async Task<ResponseDto> CreateResource([FromBody] ResourceDto resourceDto)
    {
        var id = User.FindFirst("userid")?.Value;
        return await _resourceService.CreateResource(resourceDto,Guid.Parse(id));
    }
}