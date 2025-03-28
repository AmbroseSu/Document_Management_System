using BusinessObject;
using DataAccess.DAO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class DigitalCertificateRepository : IDigitalCertificateRepository
{
    private readonly BaseDao<DigitalCertificate> _digitalCetificateDao;


    public async Task AddAsync(DigitalCertificate entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _digitalCetificateDao.AddAsync(entity);
    }

    public async Task UpdateAsync(DigitalCertificate entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _digitalCetificateDao.UpdateAsync(entity);
    }

    public async Task<DigitalCertificate?> FindDigitalCertificateByIdlAsync(Guid? id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _digitalCetificateDao.FindByAsync(dc => dc.DigitalCertificateId == id && dc.IsRevoked == false);
    }

    public async Task<IEnumerable<DigitalCertificate>?> FindAllDigitalCertificateByIdAsync(Guid? id)
    {
        return await _digitalCetificateDao.FindAsync(dc => dc.DigitalCertificateId == id && dc.IsRevoked == false);
    }
}