using BusinessObject;
using Task = System.Threading.Tasks.Task;

namespace Repository;

public interface IDigitalCertificateRepository
{
    Task AddAsync(DigitalCertificate entity);
    Task UpdateAsync(DigitalCertificate entity);
    Task<DigitalCertificate?> FindDigitalCertificateByIdlAsync(Guid? id);
    Task<IEnumerable<DigitalCertificate>?> FindAllDigitalCertificateByIdAsync(Guid? id);
}