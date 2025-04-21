using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class AuthenticationService : IAuthenticationService
{
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<ResponseDto> SignIn(SignInRequest signInRequest)
    {
        try
        {
            /*if (!IsValidEmail(signInRequest.Email))
            {
                return ResponseUtil.Error("Invalid email format", "Failed", HttpStatusCode.BadRequest);
            }*/
            var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(signInRequest.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.Password))
                return ResponseUtil.Error(ResponseMessages.EmailNotExists + " or " + ResponseMessages.PasswordNotExists,
                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);

            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (!signInRequest.FcmToken.Equals(user.FcmToken) && !signInRequest.FcmToken.Equals("string"))
                user.FcmToken = signInRequest.FcmToken;
            //await _UnitOfWork.UserUOW.(user);
            var userRoles = await _unitOfWork.UserRoleUOW.FindRolesByUserIdAsync(user.UserId);
            var roleNames = new List<string>();
            var roles = new List<RoleDto>();
            var resources = new List<Resource>();
            foreach (var userRole in userRoles)
            {
                var roleResources = await _unitOfWork.RoleResourceUOW.FindRoleResourcesByRoleIdAsync(userRole.RoleId);
                var role = await _unitOfWork.RoleUOW.FindRoleByIdAsync(userRole.RoleId);
                roleNames.Add(role.RoleName);
                var roleDto = _mapper.Map<RoleDto>(role);
                roles.Add(roleDto);
                foreach (var roleResource in roleResources)
                {
                    var resource = await _unitOfWork.ResourceUOW.FindResourceByIdAsync(roleResource.ResourceId);
                    if (resource != null) resources.Add(resource);
                }
            }

            var resourceNames = resources.Select(r => r.ResourceName).Distinct().ToList();


            var jwt = _jwtService.GenerateToken(user, roleNames, resourceNames!);
            var refreshToken = _jwtService.GenerateRefreshToken(user, new Dictionary<string, object>());

            var jwtAuthResponse = new JwtAuthenticationResponse();
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;
            var division = await _unitOfWork.DivisionUOW.FindDivisionByIdAsync(user.DivisionId);
            if (division != null)
            {
                var divisionDto = _mapper.Map<DivisionDto>(division);
                userDto.DivisionDto = divisionDto;
            }

            jwtAuthResponse.UserDto = userDto;
            jwtAuthResponse.Token = jwt;
            jwtAuthResponse.RefreshToken = refreshToken;
            if (!signInRequest.FcmToken.Equals("string") || signInRequest.FcmToken != null)
                user.FcmToken = signInRequest.FcmToken;
            await _unitOfWork.UserUOW.UpdateAsync(user);
            return ResponseUtil.GetObject(jwtAuthResponse, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created,
                0);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> SendOtpAsync(string email)
    {
        try
        {
            var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(email);
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

            return ResponseUtil.GetObject(ResponseMessages.SendOtpSuccess, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ResponseDto> VerifyOtpAsync(VerifyOtpRequest verifyOtpRequest)
    {
        try
        {
            var verificationOtp = await _unitOfWork.VerificationOtpUOW.FindByTokenAsync(verifyOtpRequest.OtpCode);
            if (verificationOtp == null)
                return ResponseUtil.Error(ResponseMessages.OtpNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (verificationOtp.IsDeleted)
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

            if (verificationOtp.Otp != verifyOtpRequest.OtpCode)
            {
                verificationOtp.AttemptCount++;
                await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.Error(ResponseMessages.OtpNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(verifyOtpRequest.Email);
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
            await _unitOfWork.VerificationOtpUOW.UpdateAsync(verificationOtp);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.OtpVerified, ResponseMessages.CreatedSuccessfully,
                HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest)
    {
        try
        {
            var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(changePasswordRequest.Email);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.EmailNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var otp = await _unitOfWork.VerificationOtpUOW.FindByUserIdAsync(user.UserId);
            if (otp == null || otp.IsTrue == false || otp.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.OtpNotVerified, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (otp.Otp != changePasswordRequest.OtpCode)
                return ResponseUtil.Error(ResponseMessages.OtpNotmatch, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (changePasswordRequest.NewPassword != changePasswordRequest.ConfirmPassword)
                return ResponseUtil.Error(ResponseMessages.PasswordNotMatchConfirm, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (!BCrypt.Net.BCrypt.Verify(changePasswordRequest.OldPassword, user.Password))
                return ResponseUtil.Error(ResponseMessages.OldPasswordIncorrect, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            
            string error;
            bool isValid = IsValidPassword(changePasswordRequest.NewPassword, out error);
            if (!isValid)
                return ResponseUtil.Error(error, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);
            await _unitOfWork.UserUOW.UpdateAsync(user);
            otp.IsDeleted = true;
            await _unitOfWork.VerificationOtpUOW.UpdateAsync(otp);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.PasswordChangeSuccess, ResponseMessages.UpdateSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
    {
        try
        {
            var user = await _unitOfWork.UserUOW.FindUserByEmailAsync(forgotPasswordRequest.Email);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.EmailNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var otp = await _unitOfWork.VerificationOtpUOW.FindByUserIdAsync(user.UserId);
            if (otp == null || otp.IsTrue == false || otp.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.OtpNotVerified, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (otp.Otp != forgotPasswordRequest.OtpCode)
                return ResponseUtil.Error(ResponseMessages.OtpNotmatch, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (forgotPasswordRequest.NewPassword != forgotPasswordRequest.ConfirmPassword)
                return ResponseUtil.Error(ResponseMessages.PasswordNotMatchConfirm, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            
            string error;
            bool isValid = IsValidPassword(forgotPasswordRequest.NewPassword, out error);
            if (!isValid)
                return ResponseUtil.Error(error, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            user.Password = BCrypt.Net.BCrypt.HashPassword(forgotPasswordRequest.NewPassword);
            await _unitOfWork.UserUOW.UpdateAsync(user);
            otp.IsDeleted = true;
            await _unitOfWork.VerificationOtpUOW.UpdateAsync(otp);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.PasswordChangeSuccess, ResponseMessages.UpdateSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }

    private int GenerateOtp()
    {
        var random = new Random();
        var otp = random.Next(100000, 999999); // Tạo một số ngẫu nhiên từ 100000 đến 999999
        return otp;
    }
    
    public bool IsValidPassword(string password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Password không được để trống.";
            return false;
        }

        if (password.Length < 8)
            errorMessage = "Password phải có ít nhất 8 ký tự.";
        else if (!password.Any(char.IsUpper))
            errorMessage = "Password phải có ít nhất một chữ cái viết hoa.";
        else if (!password.Any(char.IsLower))
            errorMessage = "Password phải có ít nhất một chữ cái thường.";
        else if (!password.Any(char.IsDigit))
            errorMessage = "Password phải có ít nhất một chữ số.";
        else if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:'\",.<>/?".Contains(ch)))
            errorMessage = "Password phải có ít nhất một ký tự đặc biệt.";
        else
            return true; // hợp lệ

        return false;
    }
}