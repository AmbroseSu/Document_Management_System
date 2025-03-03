namespace Repository;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserUOW { get; }
    IResourceRepository ResourceUOW { get; }
    IPermissionRepository PermissionUOW { get; }
    IRoleRepository RoleUOW { get; }
    IRoleResourceRepository RoleResourceUOW { get; }
   
    Task<int> SaveChangesAsync();
}