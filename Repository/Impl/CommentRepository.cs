using BusinessObject;
using DataAccess;
using DataAccess.DAO;

namespace Repository.Impl;

public class CommentRepository : ICommentRepository
{
    private readonly BaseDao<Comment> _commentDao;

    public CommentRepository(DocumentManagementSystemDbContext context)
    {
        _commentDao = new BaseDao<Comment>(context ?? throw new ArgumentNullException(nameof(context)));
    }
    
    public async Task AddAsync(Comment comment)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));
        await _commentDao.AddAsync(comment);
    }
    
    public async Task UpdateAsync(Comment comment)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));
        await _commentDao.UpdateAsync(comment);
    }
    
}