using System.Net;
using System.Text;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Newtonsoft.Json;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class SignApiService
{
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;

    public SignApiService(IFileService fileService, IUnitOfWork unitOfWork)
    {
        _fileService = fileService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResponseDto> SignApi(Guid userId, SignRequest signRequest)
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
            var userNameBase64 = Environment.GetEnvironmentVariable("SIGN_USERNAME")!;
            var passwordBase64 = Environment.GetEnvironmentVariable("SIGN_PASSWORD")!;
            var userName = Encoding.UTF8.GetString(Convert.FromBase64String(userNameBase64));
            var password = Encoding.UTF8.GetString(Convert.FromBase64String(passwordBase64));
            var json = $"{{\"username\": \"{userName}\", \"password\": \"{password}\"}}";
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
            var coordinateString = GetCoordinateString(signRequest.Llx, signRequest.Lly,
                signRequest.Urx, signRequest.Ury);
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(signRequest.DocumentId);
            
            
            
            return null;
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
}