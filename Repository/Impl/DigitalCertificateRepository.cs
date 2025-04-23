using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class DigitalCertificateRepository : IDigitalCertificateRepository
{
    private readonly BaseDao<DigitalCertificate> _digitalCetificateDao;

    public DigitalCertificateRepository(DocumentManagementSystemDbContext context)
    {
        _digitalCetificateDao = new BaseDao<DigitalCertificate>(context ?? throw new ArgumentNullException(nameof(context)));
    }

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
        return await _digitalCetificateDao.FindByAsync(dc => dc.DigitalCertificateId == id);
    }

    public async Task<IEnumerable<DigitalCertificate>?> FindAllDigitalCertificateByIdAsync(Guid? id)
    {
        return await _digitalCetificateDao.FindAsync(dc => dc.DigitalCertificateId == id);
    }
    
    public async Task<DigitalCertificate?> FindDigitalCertificateBySerialnumberlAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber)) throw new ArgumentNullException(nameof(serialNumber));
        return await _digitalCetificateDao.FindByAsync(dc => dc.SerialNumber == serialNumber);
    }
    
    public async Task<IEnumerable<DigitalCertificate>?> FindDigitalCertificateByUserIdAsync(Guid? userId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _digitalCetificateDao.FindAsync(dc => dc.UserId == userId, dg => dg.Include(u => u.User));
    }
    
}