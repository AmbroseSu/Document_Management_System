using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class DivisionRepository : IDivisionRepository
{
    public Task AddAsync(Division entity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Division entity)
    {
        throw new NotImplementedException();
    }

    public Task<Division?> FindDivisionByIdAsync(Guid? id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Division>> FindAllUserAsync()
    {
        throw new NotImplementedException();
    }
}