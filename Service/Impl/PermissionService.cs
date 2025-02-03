using BusinessObject;
using Repository;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class PermissionService : IPermissionService
{
    
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SeedPermissionsAsync()
    {
        var defaultPermissions = new List<Permission>
        {
            new Permission { PermissionName = "Full_Access" },
            new Permission { PermissionName = "View" },
            new Permission { PermissionName = "Create" },
            new Permission { PermissionName = "Modify" },
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
}