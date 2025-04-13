using BusinessObject;
using DataAccess;
using DataAccess.DAO;
using Task = System.Threading.Tasks.Task;

namespace Repository.Impl;

public class VerificationOtpRepository : IVerificationOtpRepository
{
    private readonly BaseDao<VerificationOtp> _verificationOtpDao;

    public VerificationOtpRepository(DocumentManagementSystemDbContext context)
    {
        _verificationOtpDao = new BaseDao<VerificationOtp>(context ?? throw new ArgumentNullException(nameof(context)));
    }

    public async Task AddAsync(VerificationOtp entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _verificationOtpDao.AddAsync(entity);
    }

    public async Task UpdateAsync(VerificationOtp entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _verificationOtpDao.UpdateAsync(entity);
    }

    public async Task<VerificationOtp?> FindByTokenAsync(string otp)
    {
        if (otp == null) throw new ArgumentNullException(nameof(otp));
        return await _verificationOtpDao.FindByAsync(u => u.Otp == otp);
    }

    public async Task<VerificationOtp?> FindByUserIdAsync(Guid? userId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _verificationOtpDao.FindByAsync(u => u.UserId == userId && u.IsDeleted == false && u.IsTrue == true);
    }
}