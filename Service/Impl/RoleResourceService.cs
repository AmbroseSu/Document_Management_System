using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class RoleResourceService : IRoleResourceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

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
            var roles = await _unitOfWork.RoleUOW.GetAllAsync();
            // Lấy tất cả Resource
            var resources = await _unitOfWork.ResourceUOW.GetAllAsync();

            // Duyệt qua tất cả các Role và Resource, tạo RoleResource mới
            foreach (var role in roles)
            {
                foreach (var resource in resources)
                {
                    Guid roleResourceId = UUIDv5Generator.Generate(role.RoleId.ToString() + resource.ResourceId.ToString());
                    var existingRoleResource = await _unitOfWork.RoleResourceUOW
                        .FindRoleResourceByIdAsync(roleResourceId);
                    if (existingRoleResource == null)
                    {
                        var roleResource = new RoleResource()
                        {
                            RoleResourceId = roleResourceId, // Tạo mới GUID cho RoleResourceId
                            RoleId = role.RoleId,           // Gán RoleId từ bảng Role
                            ResourceId = resource.ResourceId, // Gán ResourceId từ bảng Resource
                            IsDeleted = true                // Đặt IsDeleted = true
                        };

                        // Thêm RoleResource vào DbContext
                        await _unitOfWork.RoleResourceUOW.AddAsync(roleResource);
                    }
                    
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
        {
            return ResponseUtil.Error(ResponseMessages.RoleWithResourceCannotBeNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
        }

        foreach (var request in roleResourceRequests)
        {
            var role = await _unitOfWork.RoleUOW.FindRoleByIdAsync(request.RoleId);
            if (role == null)
            {
                return ResponseUtil.Error(ResponseMessages.RoleNotExists + " : " + request.RoleId, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }

            if (request.ResourceIds != null)
            {
                foreach (var resourceId in request.ResourceIds)
                {
                    var resource = await _unitOfWork.ResourceUOW.FindResourceByIdAsync(resourceId);
                    if (resource == null)
                    {
                        return ResponseUtil.Error(ResponseMessages.ResourceNotExists + " : " + resourceId, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
                    }

                    var roleResourceId = UUIDv5Generator.Generate(request.RoleId.ToString() + resourceId.ToString());
                    var existingRoleResource = await _unitOfWork.RoleResourceUOW.FindRoleResourceByIdAsync(roleResourceId);

                    if (existingRoleResource != null)
                    {
                        existingRoleResource.IsDeleted = false;
                        await _unitOfWork.RoleResourceUOW.UpdateAsync(existingRoleResource);
                    }
                    else
                    {
                        return ResponseUtil.Error(ResponseMessages.RoleResourceNotExists, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
                    }
                }
            }
            else
            {
                return ResponseUtil.Error(ResponseMessages.ResourceCannotBeNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
        }

        var saveChange = await _unitOfWork.SaveChangesAsync();
        if (saveChange > 0)
        {
            return ResponseUtil.GetObject(roleResourceRequests, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
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
    
    
}