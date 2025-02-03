using DataAccess;

namespace Repository.Impl;

public class UnitOfWork : IUnitOfWork
{
    
    public IUserRepository UserUOW { get; }
    public IResourceRepository ResourceUOW { get; }
    public IPermissionRepository PermissionUOW { get; }
    private bool _disposed;
    private readonly DocumentManagementSystemDbContext _context;
    
    public UnitOfWork(IUserRepository userUow, DocumentManagementSystemDbContext context, IResourceRepository resourceUow, IPermissionRepository permissionUow)
    {
        UserUOW = userUow ?? throw new ArgumentNullException(nameof(userUow));
        _disposed = false;
        _context = context;
        ResourceUOW = resourceUow ?? throw new ArgumentNullException(nameof(resourceUow));
        PermissionUOW = permissionUow;
    }

    public async Task<int> SaveChangesAsync()
    {
        //using var context = new DocumentManagementSystemDbContext();
        //return await _context.SaveChangesAsync();
        if (_context.ChangeTracker.HasChanges())
        {
            return await _context.SaveChangesAsync();
        }
        else
        {
            // Không có thay đổi, có thể log hoặc trả về 0
            return 0;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Giải phóng các tài nguyên được quản lý
                _context?.Dispose();
            }

            _disposed = true;
        }
    }
}