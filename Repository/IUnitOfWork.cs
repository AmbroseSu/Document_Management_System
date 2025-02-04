namespace Repository;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserUOW { get; }
    IResourceRepository ResourceUOW { get; }
    IPermissionRepository PermissionUOW { get; }
    
    Task<int> SaveChangesAsync();
}