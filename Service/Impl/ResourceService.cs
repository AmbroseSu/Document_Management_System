using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Repository;
using Service.Response;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class ResourceService : IResourceService
{
    
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;

    public ResourceService(
        IResourceRepository resourceRepository,
        IActionDescriptorCollectionProvider actionDescriptorProvider, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _resourceRepository = resourceRepository;
        _actionDescriptorProvider = actionDescriptorProvider;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task ScanAndSaveResourcesAsync()
    {
        var routes = _actionDescriptorProvider.ActionDescriptors.Items
            .Where(x => x is ControllerActionDescriptor)
            .Cast<ControllerActionDescriptor>()
            .Select(x => new ResourceDto()
            {
                ResourceName = ConvertLastSegmentToTitleCase(x.AttributeRouteInfo?.Template),
                ResourceApi = $"/{x.AttributeRouteInfo?.Template}",
                ResourceMethod = GetHttpMethodFromActionConstraints(x) ?? "GET"
            })
            .ToList();

        await _unitOfWork.ResourceUOW.AddRangeAsync(routes);
        await _unitOfWork.SaveChangesAsync();
        //await _apiResourceRepository.SaveResourcesAsync(routes);
    }

    public async Task<ResponseDto> CreateResource(ResourceDto resourceDto)
    {
        try
        {
            if (resourceDto.ResourceApi == null || resourceDto.ResourceMethod == null)
            {
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            Resource? resourceCheck = _unitOfWork.ResourceUOW.FindResourceByApiAsync(resourceDto.ResourceApi).Result;
            if (resourceCheck != null)
            {
                return ResponseUtil.Error(ResponseMessages.ResourceAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            resourceDto.ResourceName = ConvertLastSegmentToTitleCase(resourceDto.ResourceApi);
            resourceDto.ResourceMethod = resourceDto.ResourceMethod.ToUpper();
            Resource resourceNew = _mapper.Map<Resource>(resourceDto);
            await _unitOfWork.ResourceUOW.AddAsync(resourceNew);
            var saveChange =  await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                var result = _mapper.Map<ResourceDto>(resourceNew);
                return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
            }
            else
            {
                return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
            }
            
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    private string ConvertLastSegmentToTitleCase(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return string.Empty;

        // Tách chuỗi theo dấu '/'
        var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return string.Empty;

        // Lấy phần tử cuối cùng (tức là phần sau dấu '/' cuối cùng)
        string lastSegment = segments[^1];

        // Thay dấu '-' thành dấu cách và viết hoa chữ cái đầu
        return string.Join(" ", lastSegment.Split('-').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
    
    private string? GetHttpMethodFromActionConstraints(ControllerActionDescriptor actionDescriptor)
    {
        // Kiểm tra các thuộc tính của hành động để xác định phương thức HTTP
        var httpMethodAttribute = actionDescriptor.MethodInfo.GetCustomAttributes(true)
            .FirstOrDefault(attr => attr is HttpMethodAttribute) as HttpMethodAttribute;

        if (httpMethodAttribute != null)
        {
            return httpMethodAttribute.HttpMethods.FirstOrDefault();
        }

        // Nếu không có thì mặc định là GET
        return "GET";
    }

}