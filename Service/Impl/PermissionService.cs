using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class PermissionService : IPermissionService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task SeedPermissionsAsync()
    {
        var defaultPermissions = new List<Permission>
        {
            new Permission { PermissionName = "Update" },
            new Permission { PermissionName = "View" },
            new Permission { PermissionName = "Create" },
            //new Permission { PermissionName = "Modify" },
            new Permission { PermissionName = "Delete" }
        };

        // Lấy danh sách Permissions hiện có từ DB qua unitOfWork
        var existingPermissions = await _unitOfWork.PermissionUOW.GetAllAsync();

        // Lọc ra các quyền mới chưa có trong DB
        var newPermissions = defaultPermissions
            .Where(dp => !existingPermissions.Any(ep => ep.PermissionName == dp.PermissionName))
            .ToList();

        if (newPermissions.Any())
        {
            // Thêm các quyền mới vào DB
            await _unitOfWork.PermissionUOW.AddRangeAsync(newPermissions);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<ResponseDto> CreatePermission(PermissionDto permissionDto)
    {
        try
        {
            if (permissionDto.PermissionName == null)
            {
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            Permission? permissionCheck = _unitOfWork.PermissionUOW.FindPermissionByNameAsync(permissionDto.PermissionName).Result;
            if (permissionCheck != null)
            {
                return ResponseUtil.Error(ResponseMessages.PermissionAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            //Permission permissionNew = new Permission { PermissionName = permissionDto.PermissionName };
            Permission permissionNew = _mapper.Map<Permission>(permissionDto);
            await _unitOfWork.PermissionUOW.AddAsync(permissionNew);
            var saveChange =  await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                var result = _mapper.Map<PermissionDto>(permissionNew);
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
}