﻿using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using BusinessObject.Enums;
using BusinessObject.Option;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
    private readonly IFileService _fileService;
    private readonly string _host;
    private readonly ILoggingService _loggingService;


    public EmailService(IConfiguration config, IUnitOfWork unitOfWork, IFileService fileService, IOptions<AppsetingOptions> options, ILoggingService loggingService)
    {
        _config = config;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _loggingService = loggingService;
        _host = options.Value.redirect_uri;
    }

    public async Task<ResponseDto> SendEmail(string emailResponse, string subject, string content)
    {
        try
        {
            if (!IsValidEmail(emailResponse))
                return ResponseUtil.Error(ResponseMessages.EmailFormatInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(emailResponse);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.EmailNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            //int otp = GenerateOTP();
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("lisa92@ethereal.email"));
            email.To.Add(MailboxAddress.Parse(emailResponse));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = content
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUserName").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
            /*var verificationToken = new VerificationOtp(otp.ToString(), user.UserId);
            verificationToken.User = user;*/

            // Save the verification token to the repository
            //await _unitOfWork.VerificationOtpUOW.AddAsync(verificationToken);
            //await _unitOfWork.SaveChangesAsync();
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

        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    private int GenerateOTP()
    {
        var random = new Random();
        var otp = random.Next(100000, 999999); // Tạo một số ngẫu nhiên từ 100000 đến 999999
        return otp;
    }
    
    public async Task<ResponseDto> SendEmailWithOAuth2(EmailRequest emailRequest,Guid userId)
    {
        var document = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(emailRequest.DocumentId);
        
        if (document.ArchivedDocumentStatus == ArchivedDocumentStatus.Withdrawn )
        {
            return ResponseUtil.Error(ResponseMessages.DocumentHadWithdrawnCanNotSend, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
        
        var doc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(document.FinalDocumentId);
        if (doc == null)
        {
            return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }

        var scope = doc.DocumentWorkflowStatuses?.FirstOrDefault(w => w.DocumentId == doc.DocumentId).Workflow.Scope;

        string token = ExchangeCodeForAccessToken(emailRequest.AccessToken).Result;
        if (token.Equals("string"))
        {
            return ResponseUtil.Error("Please login google again", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
        }
        string email = await GetEmailFromAccessToken(token);
        /*if (email != emailRequest.YourEmail)
        {
            return ResponseUtil.GetObject(ResponseMessages.EmailNotMatch, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest, 1);
        }*/
        
        var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(email);
        var userRole = await _unitOfWork.UserRoleUOW.FindRolesByUserIdAsync(user.UserId);
        var roleName = userRole.Where(r => r.IsPrimary == true).FirstOrDefault().Role.RoleName;
        if (scope != Scope.OutGoing && !roleName.ToLower().Equals("chief"))
        {
            return ResponseUtil.Error(ResponseMessages.CanNotSendEmail, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(email, email));
        // message.To.Add(new MailboxAddress(emailRequest.ReceiverEmail, emailRequest.ReceiverEmail));
        emailRequest.ReceiverEmail = emailRequest.ReceiverEmail?
            .Where(to => !string.IsNullOrWhiteSpace(to))
            .ToList();

        if (emailRequest.ReceiverEmail != null && emailRequest.ReceiverEmail.Any())
        {
            foreach (var to in emailRequest.ReceiverEmail)
            {
                message.To.Add(new MailboxAddress(to, to));
            }
        }
        else
        {
            return ResponseUtil.Error("ReceiverEmail is required", ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
        }
        message.Subject = emailRequest.Subject;
        
        emailRequest.CcEmails = emailRequest.CcEmails?
            .Where(cc => !string.IsNullOrWhiteSpace(cc))
            .ToList();
        if (emailRequest.CcEmails != null && emailRequest.CcEmails.Any())
        {
            foreach (var cc in emailRequest.CcEmails)
            {
                message.Cc.Add(new MailboxAddress(cc, cc));
            }
        }
        emailRequest.BccEmails = emailRequest.BccEmails?
            .Where(cc => !string.IsNullOrWhiteSpace(cc))
            .ToList();
        
        if (emailRequest.BccEmails != null && emailRequest.BccEmails.Any())
        {
            foreach (var bcc in emailRequest.BccEmails)
            {
                message.Bcc.Add(new MailboxAddress(bcc, bcc));
            }
        }

        // Tạo phần thân dạng Multipart (nội dung + file)
        var multipart = new Multipart("mixed");

        // Nội dung văn bản
        var textPart = new TextPart("plain") { Text = emailRequest.Body };
        multipart.Add(textPart);
        
        
        var (bytes, fileName, contentType) = await _fileService.GetFileBytes(Path.Combine("archive_document", emailRequest.DocumentId.ToString(),
            document.ArchivedDocumentName + ".pdf"));
        
        var attachment = new MimePart()
        {
            Content = new MimeContent(new MemoryStream(bytes)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = fileName
        };

        multipart.Add(attachment);
        //var memoryStream = new MemoryStream();
        // Nếu có file đính kèm thì thêm vào
        /*if (emailRequest.FilePath != null && emailRequest.FilePath.Length > 0)
        {
            
            await emailRequest.FilePath.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var attachment = new MimePart()
            {
                Content = new MimeContent(memoryStream),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = emailRequest.FilePath.FileName
            };
            multipart.Add(attachment);
        }*/

        message.Body = multipart;

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            var oauth2 = new SaslMechanismOAuth2(email, token);
            await client.AuthenticateAsync(oauth2);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);


            if (document.ArchivedDocumentStatus == ArchivedDocumentStatus.Archived && document.DocumentRevokeId == null)
            {
                document.ArchivedDocumentStatus = ArchivedDocumentStatus.Sent;
                document.DateSented = DateTime.UtcNow;
                document.Sender = email;
                await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(document);
            }

            if (document.DocumentRevokeId != null && document.ArchivedDocumentStatus == ArchivedDocumentStatus.Archived)
            {
                
                
                var documentRevoke = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(document.DocumentRevokeId);
                if (documentRevoke != null)
                {
                    documentRevoke.ArchivedDocumentStatus = ArchivedDocumentStatus.Withdrawn;
                    documentRevoke.DateSented = DateTime.UtcNow;
                    documentRevoke.Sender = email;
                    await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(documentRevoke);
                    document.ArchivedDocumentStatus = ArchivedDocumentStatus.Sent;
                    document.DateSented = DateTime.UtcNow;
                    document.Sender = email;
                    await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(document);
                }
            }
            ////
            
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
        /*finally
        {
            memoryStream.Dispose();
        }*/
        await _loggingService.WriteLogAsync(userId,$"Đã gửi Email với thông tin: {emailRequest}, thành công.");
        return ResponseUtil.GetObject(ResponseMessages.SendEmailSuccessfully, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }
    
    private async Task<string> ExchangeCodeForAccessToken(string code)
    {
        var httpClient = new HttpClient();
        var values = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", Environment.GetEnvironmentVariable("CLIENT_ID")! },
            { "client_secret", Environment.GetEnvironmentVariable("CLIENT_SECRET")! },
            { "redirect_uri", "https://www.signdoc-core.io.vn/send-email" },
            //{ "redirect_uri", "http://localhost:3000/send-email" },
            //{ "redirect_uri", "http://127.0.0.1:5500/test.html" },
            { "grant_type", "authorization_code" }
        };

        var content = new FormUrlEncodedContent(values);
        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonDocument.Parse(responseString);
        var accessToken = tokenResponse.RootElement.GetProperty("access_token").GetString();
        
        return accessToken == null ? "string" : accessToken;
        //return responseString; // chứa access_token, refresh_token, ...
    }
    
    private async Task<string> GetEmailFromAccessToken(string accessToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    
        var response = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        var responseString = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(responseString);
        string email = json.RootElement.GetProperty("email").GetString();

        return email;
    }
}


