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
        /*var routes = _actionDescriptorProvider.ActionDescriptors.Items
            .Where(x => x is ControllerActionDescriptor)
            .Cast<ControllerActionDescriptor>()
            .Select(x => new ResourceDto()
            {
                ResourceName = ConvertLastSegmentToTitleCase(x.AttributeRouteInfo?.Template),
                ResourceApi = $"/{x.AttributeRouteInfo?.Template}",
                PermissionId = ConvertApiToAction(x.AttributeRouteInfo?.Template).Result,
            })
            .ToList();

        await _unitOfWork.ResourceUOW.AddRangeAsync(routes);
        await _unitOfWork.SaveChangesAsync();*/
        //await _apiResourceRepository.SaveResourcesAsync(routes);
        try
        {
            var routes = new List<ResourceDto>();

            foreach (var x in _actionDescriptorProvider.ActionDescriptors.Items
                         .Where(x => x is ControllerActionDescriptor)
                         .Cast<ControllerActionDescriptor>())
            {
                var permissionId = await ConvertApiToAction(x.AttributeRouteInfo?.Template);

                if (permissionId == Guid.Empty)
                {
                    throw new Exception($"Không tìm thấy quyền phù hợp cho API: {x.AttributeRouteInfo?.Template}");
                }

                routes.Add(new ResourceDto()
                {
                    ResourceName = ConvertLastSegmentToTitleCase(x.AttributeRouteInfo?.Template),
                    ResourceApi = $"/{x.AttributeRouteInfo?.Template}",
                    PermissionId = permissionId,
                });
            }

            await _unitOfWork.ResourceUOW.AddRangeAsync(routes);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi và có thể throw lại nếu cần
            Console.WriteLine($"Lỗi khi quét và lưu API: {ex.Message}");
            throw;
        }
    }
    
    public async Task<ResponseDto> CreateResource(ResourceDto resourceDto)
    {
        try
        {
            if (resourceDto.ResourceApi == null)
            {
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            var permissionId = await ConvertApiToAction(resourceDto.ResourceApi);
            if (permissionId == Guid.Empty)
            {
                return ResponseUtil.Error(ResponseMessages.PermissionNotExistsWithApi + $": {resourceDto.ResourceApi}", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            if (resourceDto.PermissionId != permissionId)
            {
                return ResponseUtil.Error(ResponseMessages.PermissionNotMatch, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            Resource? resourceCheck = _unitOfWork.ResourceUOW.FindResourceByApiAsync(resourceDto.ResourceApi).Result;
            if (resourceCheck != null)
            {
                return ResponseUtil.Error(ResponseMessages.ResourceAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            resourceDto.ResourceName = ConvertLastSegmentToTitleCase(resourceDto.ResourceApi);
            resourceDto.PermissionId = permissionId;
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

        // Lấy phần tử cuối cùng (nếu có)
        string lastSegment = segments[^1];

        // Lấy phần tử trước cuối (nếu có), nếu không thì bỏ qua
        string secondLastSegment = segments.Length > 1 ? segments[^2] : string.Empty;

        // Kiểm tra xem lastSegment có dữ liệu không
        if (string.IsNullOrWhiteSpace(lastSegment))
            return string.Empty;

        // Chuyển đổi từng phần tử thành chữ hoa chữ cái đầu
        string formattedLastSegment = string.Join(" ", lastSegment.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));

        string formattedSecondLastSegment = string.IsNullOrWhiteSpace(secondLastSegment)
            ? string.Empty
            : $"[{char.ToUpper(secondLastSegment[0]) + secondLastSegment.Substring(1).ToLower()}] ";

        return $"{formattedSecondLastSegment}{formattedLastSegment}".Trim();
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
    
    private async Task<Guid> ConvertApiToAction(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return Guid.Empty;

        // Tách chuỗi theo dấu '/'
        var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return Guid.Empty;

        // Lấy phần tử cuối cùng
        string lastSegment = segments[^1];

        // Tách theo dấu '-' và lấy phần đầu tiên (ví dụ: "create-user" -> "create")
        string action = lastSegment.Split('-').FirstOrDefault() ?? string.Empty;

        Permission? permissionResult = await _unitOfWork.PermissionUOW.FindPermissionByNameAsync(action);

        if (permissionResult != null)
        {
            return permissionResult.PermissionId;
        }
        else
        {
            return Guid.Empty;
        }
    }

}