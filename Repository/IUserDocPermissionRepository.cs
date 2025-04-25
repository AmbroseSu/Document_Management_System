using BusinessObject;

namespace Repository;

public interface IUserDocPermissionRepository
{
    Task AddAsync(UserDocumentPermission entity);
    Task AddRangeAsync(List<UserDocumentPermission> userDocumentPermissions);
    Task<bool> ExistsAsync(Guid userId, Guid archivedDocumentId);
}