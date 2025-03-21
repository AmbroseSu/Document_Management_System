using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IVerificationOtpRepository
{
    Task AddAsync(VerificationOtp verificationToken);
    Task UpdateAsync(VerificationOtp verificationToken);
    Task<VerificationOtp?> FindByTokenAsync(string otp);
    Task<VerificationOtp?> FindByUserIdAsync(Guid? userId);
}