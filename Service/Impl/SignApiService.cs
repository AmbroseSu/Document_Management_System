using System.Linq.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class SignApiService : ISignApiService
{
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage");

    public SignApiService(IFileService fileService, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _fileService = fileService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }


    public async Task<ResponseDto> SignInSignature(Guid userId, SignInSignature signInSignature)
    {
        try
        {
            var client = new HttpClient();
            var json = $"{{\"username\": \"{signInSignature.UserName}\", \"password\": \"{signInSignature.Password}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://demohsm.wgroup.vn/hsm/auth", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return ResponseUtil.Error("Không thể xác thực với HSM.", ResponseMessages.OperationFailed,
                    HttpStatusCode.InternalServerError);
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
            var resultObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(root["result"].ToString());
            var token = resultObj.ContainsKey("token") ? resultObj["token"].ToString() : null;
            if (string.IsNullOrEmpty(token))
            {
                return ResponseUtil.Error("Không lấy được token từ phản hồi HSM.", ResponseMessages.OperationFailed,
                    HttpStatusCode.InternalServerError);
            }
            
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.EmailNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var verificationOtp = await _unitOfWork.VerificationOtpUOW.FindByUserIdAsync(user.UserId);
            if (verificationOtp != null)
            {
                /*if (verificationOtp.ExpirationTime > DateTime.Now && verificationOtp.IsDeleted == false)
                {
                    return ResponseUtil.Error(ResponseMessages.OtpHasNotExpired, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
                } */
                verificationOtp.IsDeleted = true;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
            }

            var otpCode = GenerateOtp();
            var otp = new VerificationOtp(otpCode.ToString(), user.UserId);
            otp.User = user;
            await _unitOfWork.VerificationOtpUOW.AddAsync(otp);
            await _unitOfWork.SaveChangesAsync();

            var emailContent = $"Xin chào {user.UserName},<br><br>"
                               + $"Mã OTP của bạn là: <b>{otpCode}</b><br>"
                               + $"Vui lòng nhập OTP trong vòng 3 phút trước khi hết hạn.";
            var emailSent = false;
            var retryCount = 0;
            var maxRetries = 3;

            //var emailResponse = await _emailService.SendEmail(userRequest.Email, "Tạo tài khoản thành công", emailContent);

            while (!emailSent && retryCount < maxRetries)
            {
                var emailResponse = await _emailService.SendEmail(user.Email!, "Mã OTP của bạn", emailContent);
                if (emailResponse.StatusCode == (int)HttpStatusCode.Created)
                {
                    emailSent = true;
                }
                else
                {
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            if (!emailSent)
                return ResponseUtil.Error(ResponseMessages.FailedToSendEmail, ResponseMessages.OperationFailed,
                    HttpStatusCode.InternalServerError);
            
            return ResponseUtil.GetObject(token, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
    }
    
    
    public async Task<ResponseDto> SignatureApi(Guid userId, SignRequest signRequest)
    {
        try
        {
            var verificationOtp = await _unitOfWork.VerificationOtpUOW.FindByTokenAsync(signRequest.OtpCode);
            if (verificationOtp == null)
                return ResponseUtil.Error(ResponseMessages.OtpNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (verificationOtp.AttemptCount >= 5)
            {
                verificationOtp.IsDeleted = true;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.Error(ResponseMessages.OtpLocked, ResponseMessages.OperationFailed,
                    HttpStatusCode.Forbidden);
            }
            if (verificationOtp.Otp != signRequest.OtpCode)
            {
                verificationOtp.AttemptCount++;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.Error(ResponseMessages.OtpNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.EmailNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (verificationOtp.UserId != user.UserId)
            {
                verificationOtp.AttemptCount++;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.Error(ResponseMessages.UserHasNotOtp, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            if (verificationOtp.ExpirationTime < DateTime.Now)
            {
                verificationOtp.AttemptCount++;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.Error(ResponseMessages.OtpExpired, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            if (verificationOtp.IsTrue)
            {
                verificationOtp.AttemptCount++;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.Error(ResponseMessages.OtpHasUsed, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            
            verificationOtp.IsTrue = true;
            verificationOtp.IsDeleted = true;
            await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
            await _unitOfWork.SaveChangesAsync();
            
            var client = new HttpClient();
            var coordinateString = GetCoordinateString(signRequest.Llx, signRequest.Lly,
                signRequest.Urx, signRequest.Ury);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(signRequest.DocumentId);
            if (document == null)
             {
                 return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
             }
            var listTasks = document.Tasks;
            var task = listTasks.Where(t =>
                t.UserId == userId && t.TaskType == TaskType.Sign && t.TaskStatus == TasksStatus.InProgress).FirstOrDefault();
            if (task == null)
            {
                return ResponseUtil.Error(ResponseMessages.NotYourTurnOrSign, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
            var version = document?.DocumentVersions?.Find(t => t.IsFinalVersion);
            var docFile = await _fileService.GetFileBytes(Path.Combine("document", document.DocumentId.ToString(),
                version.DocumentVersionId.ToString(),
                document.DocumentName + ".pdf"));
            var docFileBytes = docFile.FileBytes;
            string fileData = Convert.ToBase64String(docFileBytes);
            //string fileData = "";

            
            var listDigitalCertificates =
                await _unitOfWork.DigitalCertificateUOW.FindDigitalCertificateByUserIdAsync(userId);
            var digitalCertificate = listDigitalCertificates.Where(dc => dc.SerialNumber != null).FirstOrDefault();
            var signFile = await _fileService.GetFileBytes(Path.Combine("signature", digitalCertificate.DigitalCertificateId + ".png"));
            var signFileBytes = signFile.FileBytes;
            string signData = Convert.ToBase64String(signFileBytes);
            //string signData = "";
            
            
            var signOptions = new SignOptions
            {
                PageNo = signRequest.PageNumber,
                Coordinate = coordinateString,
                VisibleSignature = true,
                VisualStatus = false,
                ShowSignerInfo = false,
                SignerInfoPrefix = "",
                ShowReason = false,
                SignReasonPrefix = "Lý do:",
                SignReason = "Tôi Đồng Ý",
                ShowDateTime = false,
                DateTimePrefix = "",
                ShowLocation = false,
                LocationPrefix = "",
                Location = "",
                TextDirection = "OVERLAP",
                TextColor = "Black",
                ImageAndText = false,
                BackgroundImage = "",
                SignatureImage = signData
            };

            var signApiRequest = new SignApiRequest
            {
                Options = signOptions,
                FileData = fileData
            };
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signRequest.token);
            //var jsonBody = JsonConvert.SerializeObject(signApiRequest);
            var optionsJson = JsonConvert.SerializeObject(
                signApiRequest.Options,
                new JsonSerializerSettings
                {
                    ContractResolver = new OptionsResolver(),
                    Formatting = Formatting.None
                });
            var root = new JObject
            {
                ["options"] = JObject.Parse(optionsJson),
                ["file_data"] = signApiRequest.FileData
            };

            string jsonBody = root.ToString(Formatting.Indented);

            
            var contentRequest = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            var responseSign = await client.PostAsync("https://demohsm.wgroup.vn/hsm/pdf", contentRequest);
            var responseContent = await responseSign.Content.ReadAsStringAsync();
            if (!responseSign.IsSuccessStatusCode)
            {
                throw new Exception($"API ký lỗi: {responseSign.StatusCode} - {responseContent}");
            }
            
            var jsonSign = JObject.Parse(responseContent);
            var fileDataBase64 = jsonSign["result"]?["file_data"]?.ToString();
            if (string.IsNullOrWhiteSpace(fileDataBase64))
                throw new Exception("Không tìm thấy trường file_data trong response");
            var fileBytes = Convert.FromBase64String(fileDataBase64);
            var filePath = Path.Combine(_storagePath, Path.Combine("document", document.DocumentId.ToString(),
                version.DocumentVersionId.ToString(),
                document.DocumentName + ".pdf"));
            File.WriteAllBytesAsync(filePath, fileBytes);
            return ResponseUtil.GetObject(ResponseMessages.SignatureSuccessfully, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created,1);
            //return ResponseUtil.GetObject(fileDataBase64, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created,1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
    }
    
    
    private String GetCoordinateString(int llx, int lly, int urx, int ury)
    {
        return $"{llx},{lly},{urx},{ury}";
    }
    private int GenerateOtp()
    {
        var random = new Random();
        var otp = random.Next(100000, 999999); // Tạo một số ngẫu nhiên từ 100000 đến 999999
        return otp;
    }
}