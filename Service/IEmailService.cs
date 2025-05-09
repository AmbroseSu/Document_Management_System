using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IEmailService
{
    Task<ResponseDto> SendEmail(string email, string subject, string content);
    Task<ResponseDto> SendEmailWithOAuth2(EmailRequest emailRequest, Guid userId);
}