using System.Net;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.AspNetCore.SignalR;
using Repository;
using Service.Response;
using Service.SignalRHub;
using Service.Utilities;

namespace Service.Impl;

public class UserDocPermissionService : IUserDocPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly MongoDbService _notificationCollection;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILoggingService _loggingService;
    

    public UserDocPermissionService(IUnitOfWork unitOfWork, INotificationService notificationService, MongoDbService notificationCollection, IHubContext<NotificationHub> hubContext, ILoggingService loggingService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _notificationCollection = notificationCollection;
        _hubContext = hubContext;
        _loggingService = loggingService;
    }

    public async Task<ResponseDto> GrantPermissionForDocument(Guid userGrantId, GrantDocumentRequest grantDocumentRequest)
    {
        try
        {
            if (grantDocumentRequest == null || grantDocumentRequest.UserGrantDocuments == null || !grantDocumentRequest.UserGrantDocuments.Any())
            {
                return ResponseUtil.Error("Danh sách người dùng không được để trống.", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
        
            var userGrand = await _unitOfWork.UserDocPermissionUOW.FindByUserIdAndArchiveDocAsync(userGrantId, grantDocumentRequest.DocumentId);
        
            if (userGrand == null)
            {
                return ResponseUtil.Error("Người dùng không có quyền truy cập văn bản này.", ResponseMessages.OperationFailed, HttpStatusCode.Forbidden);
            }

            if (userGrand.GrantPermission != GrantPermission.Grant)
            {
                return ResponseUtil.Error("Người dùng không có quyền cấp quyền văn bản này cho người khác.", ResponseMessages.OperationFailed, HttpStatusCode.Forbidden);
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
            var affectedUserIds = new List<Guid>();

            foreach (var userGrant in grantDocumentRequest.UserGrantDocuments)
            {
                if (userGrant.GrantPermission == GrantPermission.Grant)
                {
                    return ResponseUtil.Error("Người dùng không được cấp quyền tổng cho người khác.", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                }
                var userId = userGrant.UserId;
                var grantPermission = userGrant.GrantPermission;
                var existing = existingPermissions.FirstOrDefault(p => p.UserId == userId);
                if (existing != null)
                {
                    if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        existing.GrantPermission = grantPermission;
                        existing.CreatedDate = DateTime.UtcNow;
                        reactivatedPermissions.Add(existing);
                        affectedUserIds.Add(userId);
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
                        GrantPermission = grantPermission,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                    affectedUserIds.Add(userId);
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

            if (affectedUserIds.Count() > 0)
            {
                foreach (var userId in affectedUserIds)
                {
                    var orUser = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
                    var notification = _notificationService.CreateArchivedDocHadGrantNotification(grantDocumentRequest.DocumentId, userId);
                    await _notificationCollection.CreateNotificationAsync(notification);
                    await _notificationService.SendPushNotificationMobileAsync(orUser.FcmToken, notification);
                    await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveMessage", notification);
                }
            }

            await _loggingService.WriteLogAsync(userGrantId,
                $"Đã cấp quyền cho văn bản {archivedDocument.SystemNumberOfDoc} với các quyền sau {grantDocumentRequest}");
            return ResponseUtil.GetObject(ResponseMessages.GrantDocumentSuccess, ResponseMessages.OperationFailed, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
}