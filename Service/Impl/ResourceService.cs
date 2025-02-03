using DataAccess.DTO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Repository;

namespace Service.Impl;

public class ResourceService : IResourceService
{
    
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;

    public ResourceService(
        IResourceRepository resourceRepository,
        IActionDescriptorCollectionProvider actionDescriptorProvider, IUnitOfWork unitOfWork)
    {
        _resourceRepository = resourceRepository;
        _actionDescriptorProvider = actionDescriptorProvider;
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.ResourceUOW.AddAsync(routes);
        await _unitOfWork.SaveChangesAsync();
        //await _apiResourceRepository.SaveResourcesAsync(routes);
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