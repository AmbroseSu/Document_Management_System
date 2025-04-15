using System.Net;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class RoleResourceService : IRoleResourceService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RoleResourceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task ScanAndSaveRoleResourcesAsync()
    {
        try
        {
            // Lấy tất cả Role
            var roles = await _unitOfWork.RoleUOW.GetAllActiveAsync();
            // Lấy tất cả Resource
            var resources = await _unitOfWork.ResourceUOW.GetAllAsync();

            // Duyệt qua tất cả các Role và Resource, tạo RoleResource mới
            foreach (var role in roles)
            foreach (var resource in resources)
            {
                var roleResourceId = UUIDv5Generator.Generate(role.RoleId + resource.ResourceId.ToString());
                var existingRoleResource = await _unitOfWork.RoleResourceUOW
                    .FindRoleResourceByIdAsync(roleResourceId);
                if (existingRoleResource == null)
                {
                    var roleResource = new RoleResource
                    {
                        RoleResourceId = roleResourceId, // Tạo mới GUID cho RoleResourceId
                        RoleId = role.RoleId, // Gán RoleId từ bảng Role
                        ResourceId = resource.ResourceId, // Gán ResourceId từ bảng Resource
                        Resource = resource, // Gán Resource   ;
                        IsDeleted = true // Đặt IsDeleted = true
                    };

                    // Thêm RoleResource vào DbContext
                    await _unitOfWork.RoleResourceUOW.AddAsync(roleResource);
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Xử lý lỗi: Ghi log lỗi hoặc báo cáo thông tin chi tiết
            Console.Error.WriteLine($"An error occurred while scanning and saving RoleResources: {ex.Message}");
            throw;
        }
    }

    public async Task<ResponseDto> UpdateRoleResourceAsync(List<RoleResourceRequest>? roleResourceRequests)
    {
        try
        {
            if (roleResourceRequests == null || !roleResourceRequests.Any())
                return ResponseUtil.Error(ResponseMessages.RoleWithResourceCannotBeNull,
                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            foreach (var request in roleResourceRequests)
            {
                var role = await _unitOfWork.RoleUOW.FindRoleByIdAsync(request.RoleId);
                if (role == null)
                    return ResponseUtil.Error(ResponseMessages.RoleNotExists + " : " + request.RoleId,
                        ResponseMessages.OperationFailed, HttpStatusCode.NotFound);

                if (request.ResourceIds != null)
                    foreach (var resourceId in request.ResourceIds)
                    {
                        var resource = await _unitOfWork.ResourceUOW.FindResourceByIdAsync(resourceId);
                        if (resource == null)
                            return ResponseUtil.Error(ResponseMessages.ResourceNotExists + " : " + resourceId,
                                ResponseMessages.OperationFailed, HttpStatusCode.NotFound);

                        var roleResourceId = UUIDv5Generator.Generate(request.RoleId + resourceId.ToString());
                        var existingRoleResource =
                            await _unitOfWork.RoleResourceUOW.FindRoleResourceByIdAsync(roleResourceId);

                        if (existingRoleResource != null)
                        {
                            if (existingRoleResource.IsDeleted)
                            {
                                existingRoleResource.IsDeleted = false;
                                await _unitOfWork.RoleResourceUOW.UpdateAsync(existingRoleResource);
                            }
                            else
                            {
                                existingRoleResource.IsDeleted = true;
                                await _unitOfWork.RoleResourceUOW.UpdateAsync(existingRoleResource);
                            }
                            
                        }
                        else
                        {
                            return ResponseUtil.Error(ResponseMessages.RoleResourceNotExists,
                                ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
                        }
                    }
                else
                    return ResponseUtil.Error(ResponseMessages.ResourceCannotBeNull, ResponseMessages.OperationFailed,
                        HttpStatusCode.BadRequest);
            }

            var saveChange = await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
                return ResponseUtil.GetObject(roleResourceRequests, ResponseMessages.CreatedSuccessfully,
                    HttpStatusCode.Created, 1);
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    // public async Task<ResponseDto> GetRoleResourceAsync(RoleFillter roleFillter)
    // {
    //     try
    //     {
    //         IEnumerable<Role> roles = Array.Empty<Role>();
    //         if (roleFillter.ToString().ToLower().Equals("Main".ToLower()))
    //         {
    //             roles = await _unitOfWork.RoleUOW.GetAllByMainRoleAsync();
    //         }
    //         else
    //         {
    //             if (roleFillter.ToString().ToLower().Equals("Sub".ToLower()))
    //             {
    //                 roles = await _unitOfWork.RoleUOW.GetAllBySubRoleAsync();
    //             }else
    //             {
    //                 if (roleFillter.ToString().ToLower().Equals("All".ToLower()))
    //                 {
    //                     roles = await _unitOfWork.RoleUOW.GetAllActiveAsync();
    //                 }
    //             }
    //         }
    //         var permissions = await _unitOfWork.PermissionUOW.GetAllAsync();
    //         
    //         List<RoleResourceResponse> roleResourceResponses = new List<RoleResourceResponse>();
    //         
    //         foreach (var role in roles)
    //         {
    //             var permissionDtos = new List<PermissionDto>();
    //             
    //             foreach (var permission in permissions)
    //             {
    //                 var resourceResponses = await GetResourceResponse(role.RoleId, permission.PermissionId);
    //                     var permissionDto = new PermissionDto
    //                     {
    //                         PermissionId = permission.PermissionId,
    //                         PermissionName = permission.PermissionName,
    //                         ResourceResponses = resourceResponses
    //                     };
    //                     permissionDtos.Add(permissionDto);
    //             }
    //             
    //             var roleResourceResponse = new RoleResourceResponse
    //             {
    //                 RoleId = role.RoleId,
    //                 RoleName = role.RoleName!,
    //                 PermissionDtos = permissionDtos
    //             };
    //             roleResourceResponses.Add(roleResourceResponse);
    //             
    //         }
    //         
    //         return ResponseUtil.GetObject(roleResourceResponses, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, roleResourceResponses.Count);
    //     }
    //     catch (Exception e)
    //     {
    //         return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
    //     }
    // }
    
    
        public async Task<ResponseDto> GetRoleResourceAsync(RoleFillter roleFillter)
    {
        try
        {
            IEnumerable<Role> roles = Array.Empty<Role>();
            if (roleFillter.ToString().ToLower().Equals("Main".ToLower()))
            {
                roles = await _unitOfWork.RoleUOW.GetAllByMainRoleAsync();
            }
            else
            {
                if (roleFillter.ToString().ToLower().Equals("Sub".ToLower()))
                {
                    roles = await _unitOfWork.RoleUOW.GetAllBySubRoleAsync();
                }else
                {
                    if (roleFillter.ToString().ToLower().Equals("All".ToLower()))
                    {
                        roles = await _unitOfWork.RoleUOW.GetAllActiveAsync();
                    }
                }
            }
            var permissions = await _unitOfWork.PermissionUOW.GetAllAsync();
            
            //List<RoleResourceResponse> roleResourceResponses = new List<RoleResourceResponse>();
            
            var roleIds = roles.Select(r => r.RoleId).ToList();
            var roleResources = await _unitOfWork.RoleResourceUOW.FindAllRoleResourcesByRoleIdsAsync(roleIds);
            var roleResourceDict = roleResources
                .GroupBy(rr => (rr.RoleId, rr.Resource.PermissionId))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(rr => new ResourceResponse
                    {
                        ResourceId = rr.Resource.ResourceId,
                        ResourceApi = rr.Resource.ResourceApi,
                        ResourceName = rr.Resource.ResourceName,
                        IsDeleted = rr.IsDeleted
                    }).ToList()
                );
            
            var roleResourceResponses = roles.Select(role => new RoleResourceResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName!,
                PermissionDtos = permissions.Select(permission => new PermissionDto
                {
                    PermissionId = permission.PermissionId,
                    PermissionName = permission.PermissionName,
                    ResourceResponses = roleResourceDict.TryGetValue((role.RoleId, permission.PermissionId), out var resources)
                        ? resources
                        : new List<ResourceResponse>()
                }).ToList()
            }).ToList();
            
            return ResponseUtil.GetObject(roleResourceResponses, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, roleResourceResponses.Count);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    
    /*private async Task<List<ResourceResponse>> GetResourceResponse(Guid roleId, Guid permissionId)
    {
        var roleResources = await _unitOfWork.RoleResourceUOW
            .FindAllRoleResourcesByRoleIdAsync(roleId, permissionId);
        
        if (!roleResources.Any()) return new List<ResourceResponse>();
        
        // var permissionDict = await _unitOfWork.PermissionUOW
        //     .GetAllAsync()
        //     .ContinueWith(t => t.Result.ToDictionary(p => p.PermissionId, p => p));
        
        // var resourceResponses = roleResources
        //     .Where(rr => permissionDict.ContainsKey(rr.Resource.PermissionId) && rr.Resource.PermissionId == permissionId)
        //     .Select(rr => new ResourceResponse
        //     {
        //         ResourceId = rr.Resource.ResourceId,
        //         ResourceApi = rr.Resource.ResourceApi,
        //         ResourceName = rr.Resource.ResourceName,
        //         IsDeleted = rr.IsDeleted
        //     })
        //     .ToList();
    
        // return resourceResponses;
        return roleResources.Select(rr => new ResourceResponse
        {
            ResourceId = rr.Resource.ResourceId,
            ResourceApi = rr.Resource.ResourceApi,
            ResourceName = rr.Resource.ResourceName,
            IsDeleted = rr.IsDeleted
        }).ToList();
    }*/
    
    
    
   

    
    
}