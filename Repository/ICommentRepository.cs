using BusinessObject;

namespace Repository;

public interface ICommentRepository
{
    Task AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
}