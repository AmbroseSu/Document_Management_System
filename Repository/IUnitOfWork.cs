namespace Repository;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserUOW { get; }
    
    Task<int> SaveChangesAsync();
}