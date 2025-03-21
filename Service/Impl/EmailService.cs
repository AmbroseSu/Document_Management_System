using System.Net;
using System.Text.RegularExpressions;
using BusinessObject;
using DataAccess.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;


    public EmailService(IConfiguration config, IUnitOfWork unitOfWork)
    {
        _config = config;
        _unitOfWork = unitOfWork;
    }
    public async Task<ResponseDto> SendEmail(String emailResponse)
    {
        try
        {
            if (IsValidEmail(emailResponse) == false)
            {
                return ResponseUtil.Error(ResponseMessages.EmailFormatInvalid, "Failed", HttpStatusCode.BadRequest);
            }
            int otp = GenerateOTP();
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("lisa92@ethereal.email"));
            email.To.Add(MailboxAddress.Parse(emailResponse));
            email.Subject = "OTP ECommerce";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = otp.ToString()
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection( "EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate( _config.GetSection("EmailUserName").Value, _config.GetSection("EmailPassword").Value );
            smtp.Send(email);
            smtp.Disconnect(true);
            //var verificationToken = new VerificationOtp(otp.ToString(), user.UserId);
            //verificationToken.User = user;

            // Save the verification token to the repository
            //await _unitOfWork.VerificationOtpUOW.AddAsync(verificationToken);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject("Send email Success", "ok", HttpStatusCode.Created, 0);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, "Failed", HttpStatusCode.InternalServerError);
        }
        
    }
    
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
    
    private int GenerateOTP()
    {
        Random random = new Random();
        int otp = random.Next(100000, 999999); // Tạo một số ngẫu nhiên từ 100000 đến 999999
        return otp;
    }
}