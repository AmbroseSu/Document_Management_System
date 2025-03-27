using DataAccess.DTO;

namespace Service;

public interface IEmailService
{
    Task<ResponseDto> SendEmail(string email, string subject, string content);
}