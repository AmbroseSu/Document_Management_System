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
    private readonly IMapper _mapper;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _logger;

    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task SeedPermissionsAsync()
    {
        var defaultPermissions = new List<Permission>
        {
            new() { PermissionName = "Update" },
            new() { PermissionName = "View" },
            new() { PermissionName = "Create" },
            //new Permission { PermissionName = "Modify" },
            new() { PermissionName = "Delete" }
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

    public async Task<ResponseDto> CreatePermission(PermissionDto permissionDto, Guid userId)
    {
        try
        {
            if (permissionDto.PermissionName == null)
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            var permissionCheck = _unitOfWork.PermissionUOW.FindPermissionByNameAsync(permissionDto.PermissionName)
                .Result;
            if (permissionCheck != null)
                return ResponseUtil.Error(ResponseMessages.PermissionAlreadyExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            //Permission permissionNew = new Permission { PermissionName = permissionDto.PermissionName };
            var permissionNew = _mapper.Map<Permission>(permissionDto);
            await _unitOfWork.PermissionUOW.AddAsync(permissionNew);
            var saveChange = await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                var result = _mapper.Map<PermissionDto>(permissionNew);
                await _logger.WriteLogAsync(userId,$"Tạo quyền {permissionDto.PermissionName} thành công");
                return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
            }
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
}