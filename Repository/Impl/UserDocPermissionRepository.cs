using BusinessObject;
using DataAccess;
using DataAccess.DAO;

namespace Repository.Impl;

public class UserDocPermissionRepository : IUserDocPermissionRepository
{
    private readonly BaseDao<UserDocumentPermission> _userDocumentPermission;

    public UserDocPermissionRepository(DocumentManagementSystemDbContext context)
    {
        _userDocumentPermission = new BaseDao<UserDocumentPermission>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(UserDocumentPermission entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _userDocumentPermission.AddAsync(entity);
    }
    
    public async Task AddRangeAsync(List<UserDocumentPermission> userDocumentPermissions)
    {
        if (userDocumentPermissions == null) throw new ArgumentNullException(nameof(userDocumentPermissions));
        await _userDocumentPermission.AddRangeAsync(userDocumentPermissions);
    }
    
    public async Task UpdateRangeAsync(List<UserDocumentPermission> userDocumentPermissions)
    {
        if (userDocumentPermissions == null) throw new ArgumentNullException(nameof(userDocumentPermissions));
        await _userDocumentPermission.UpdateRangeAsync(userDocumentPermissions);
    }

    
    public async Task<bool> ExistsAsync(Guid userId, Guid archivedDocumentId)
    {
        var existing = await _userDocumentPermission.FindByAsync(x =>
            x.UserId == userId &&
            x.ArchivedDocumentId == archivedDocumentId &&
            !x.IsDeleted);
        
        return existing != null;
    }

    public async Task<IEnumerable<UserDocumentPermission>> GetPermissionsByDocumentIdAsync(Guid archivedDocumentId)
    {
        return await _userDocumentPermission.FindAsync(udp => udp.ArchivedDocumentId == archivedDocumentId);
    }
    
    public async Task<UserDocumentPermission?> FindByUserIdAndArchiveDocAsync(Guid? userId, Guid? archivedDocumentId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (archivedDocumentId == null) throw new ArgumentNullException(nameof(archivedDocumentId));

        return await _userDocumentPermission.FindByAsync(x =>
            x.UserId == userId &&
            x.ArchivedDocumentId == archivedDocumentId &&
            !x.IsDeleted);
    }
}