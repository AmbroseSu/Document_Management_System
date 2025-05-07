using BusinessObject;

namespace Repository;

public interface IUserDocPermissionRepository
{
    Task AddAsync(UserDocumentPermission entity);
    Task AddRangeAsync(List<UserDocumentPermission> userDocumentPermissions);
    Task<bool> ExistsAsync(Guid userId, Guid archivedDocumentId);
    Task UpdateRangeAsync(List<UserDocumentPermission> userDocumentPermissions);
    Task UpdateAsync(UserDocumentPermission userDocumentPermissions);

    Task<IEnumerable<UserDocumentPermission>> GetPermissionsByDocumentIdAsync(Guid archivedDocumentId);
    Task<UserDocumentPermission?> FindByUserIdAndArchiveDocAsync(Guid? userId, Guid? archivedDocumentId);
}