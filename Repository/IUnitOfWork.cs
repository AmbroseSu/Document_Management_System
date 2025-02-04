namespace Repository;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserUOW { get; }
    IResourceRepository ResourceUOW { get; }
    IPermissionRepository PermissionUOW { get; }
    IRoleRepository RoleUOW { get; }
    IRolePermissionRepository RolePermissionUOW { get; }
    
    Task<int> SaveChangesAsync();
}