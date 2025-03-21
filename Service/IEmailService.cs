using DataAccess.DTO;

namespace Service;

public interface IEmailService
{
    Task<ResponseDto> SendEmail(String email);
}