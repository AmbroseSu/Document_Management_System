namespace Repository;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserUOW { get; }
    IResourceRepository ResourceUOW { get; }
    IPermissionRepository PermissionUOW { get; }
    IRoleRepository RoleUOW { get; }
    IRoleResourceRepository RoleResourceUOW { get; }
    IUserRoleRepository UserRoleUOW { get; }
    IVerificationOtpRepository VerificationOtpUOW { get; }
    IDigitalCertificateRepository DigitalCertificateUOW { get; }
    IDivisionRepository DivisionUOW { get; }

    Task<int> SaveChangesAsync();
}