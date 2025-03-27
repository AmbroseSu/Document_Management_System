using DataAccess;

namespace Repository.Impl;

public class UnitOfWork : IUnitOfWork
{
    private readonly DocumentManagementSystemDbContext _context;
    private bool _disposed;

    public UnitOfWork(IUserRepository userUow, DocumentManagementSystemDbContext context,
        IResourceRepository resourceUow, IPermissionRepository permissionUow, IRoleRepository roleUow,
        IRoleResourceRepository roleResourceUow, IUserRoleRepository userRoleUow,
        IVerificationOtpRepository verificationOtpUow, IDigitalCertificateRepository digitalCertificateUow,
        IDivisionRepository divisionUow)
    {
        UserUOW = userUow ?? throw new ArgumentNullException(nameof(userUow));
        _disposed = false;
        _context = context;
        ResourceUOW = resourceUow ?? throw new ArgumentNullException(nameof(resourceUow));
        PermissionUOW = permissionUow ?? throw new ArgumentNullException(nameof(permissionUow));
        ;
        RoleUOW = roleUow ?? throw new ArgumentNullException(nameof(roleUow));
        RoleResourceUOW = roleResourceUow;
        UserRoleUOW = userRoleUow;
        VerificationOtpUOW = verificationOtpUow;
        DigitalCertificateUOW = digitalCertificateUow;
        DivisionUOW = divisionUow;
    }

    public IUserRepository UserUOW { get; }
    public IResourceRepository ResourceUOW { get; }
    public IPermissionRepository PermissionUOW { get; }
    public IRoleRepository RoleUOW { get; }
    public IRoleResourceRepository RoleResourceUOW { get; }
    public IUserRoleRepository UserRoleUOW { get; }
    public IVerificationOtpRepository VerificationOtpUOW { get; }
    public IDigitalCertificateRepository DigitalCertificateUOW { get; }
    public IDivisionRepository DivisionUOW { get; }

    public async Task<int> SaveChangesAsync()
    {
        //using var context = new DocumentManagementSystemDbContext();
        //return await _context.SaveChangesAsync();
        if (_context.ChangeTracker.HasChanges())
            return await _context.SaveChangesAsync();
        // Không có thay đổi, có thể log hoặc trả về 0
        return 0;
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
                // Giải phóng các tài nguyên được quản lý
                _context?.Dispose();

            _disposed = true;
        }
    }
}