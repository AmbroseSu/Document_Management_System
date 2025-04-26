using System.Net;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class UserDocPermissionService : IUserDocPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserDocPermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto> GrantPermissionForDocument(GrantDocumentRequest grantDocumentRequest)
    {
        try
    {
        if (grantDocumentRequest == null || grantDocumentRequest.UserIds == null || !grantDocumentRequest.UserIds.Any())
        {
            return ResponseUtil.Error("Danh sách người dùng không được để trống.", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
        }

        var archivedDocument = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(grantDocumentRequest.DocumentId);
        if (archivedDocument == null)
        {
            return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
        }

        // Lấy tất cả quyền (cả IsDeleted true/false)
        var existingPermissions = (await _unitOfWork.UserDocPermissionUOW
            .GetPermissionsByDocumentIdAsync(grantDocumentRequest.DocumentId)).ToList();

        var reactivatedPermissions = new List<UserDocumentPermission>();
        var newPermissions = new List<UserDocumentPermission>();

        foreach (var userId in grantDocumentRequest.UserIds)
        {
            var existing = existingPermissions.FirstOrDefault(p => p.UserId == userId);
            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.CreatedDate = DateTime.UtcNow;
                    reactivatedPermissions.Add(existing);
                }
                // Nếu đã tồn tại và không bị xóa thì bỏ qua
            }
            else
            {
                newPermissions.Add(new UserDocumentPermission
                {
                    UserDocumentPermissionId = Guid.NewGuid(),
                    ArchivedDocumentId = grantDocumentRequest.DocumentId,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }

        if (!newPermissions.Any() && !reactivatedPermissions.Any())
        {
            return ResponseUtil.Error("Tất cả người dùng đã có quyền truy cập văn bản này.", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
        }

        if (newPermissions.Any())
            await _unitOfWork.UserDocPermissionUOW.AddRangeAsync(newPermissions);

        if (reactivatedPermissions.Any())
            await _unitOfWork.UserDocPermissionUOW.UpdateRangeAsync(reactivatedPermissions); // Cần thêm hàm UpdateRangeAsync nếu chưa có

        await _unitOfWork.SaveChangesAsync();

        return ResponseUtil.GetObject(ResponseMessages.GrantDocumentSuccess, ResponseMessages.OperationFailed, HttpStatusCode.OK, 1);
    }
    catch (Exception e)
    {
        return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
    }
    }
}