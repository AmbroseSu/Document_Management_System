using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using DataAccess.DAO;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Npgsql.Replication;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<ResponseDto> CreateUserByForm(UserRequest userRequest)
    {
        try
        {
            if (userRequest == null)
            {
                throw new ArgumentNullException(nameof(userRequest));
            }
            
            User? userExistEmail = await _unitOfWork.UserUOW.FindUserByEmailAsync(userRequest.Email);
            if (userExistEmail != null)
            {
                return ResponseUtil.Error(ResponseMessages.EmailAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            if (userExistEmail!.IsDeleted)
            {
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            //var emailAttribute = new EmailAddressAttribute();
            /*if (IsValidEmail(userRequest.Email))
            {
                return ResponseUtil.Error(ResponseMessages.InvalidEmailFormat, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }*/
            User? userExistUserName = await _unitOfWork.UserUOW.FindUserByUserNameAsync(userRequest.UserName);
            if (userExistUserName != null)
            {
                return ResponseUtil.Error(ResponseMessages.UserNameAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            if (userExistUserName!.IsDeleted)
            {
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            if (!IsValidEmail(userRequest.Email))
            {
                return ResponseUtil.Error(ResponseMessages.EmailFormatInvalid, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            if (!IsValidPhoneNumber(userRequest.PhoneNumber))
            {
                return ResponseUtil.Error(ResponseMessages.PhoneNumberFormatInvalid, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            string plainPassword = GenerateRandomString(10);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            
            
            
            User user = _mapper.Map<User>(userRequest);
            //user.Password = BCrypt.Net.BCrypt.HashPassword("Hieu1234");
            user.Password = hashedPassword;
            user.PhoneNumber = userRequest.PhoneNumber;
            user.IsDeleted = false;
            user.IsEnable = false;
            user.CreatedAt = DateTime.Now.ToUniversalTime();
            user.UpdatedAt = DateTime.Now.ToUniversalTime();
            await _unitOfWork.UserUOW.AddAsync(user);
            var saveChange = await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                UserRole userRole = new UserRole();
                userRole.UserId = user.UserId;
                userRole.RoleId = userRequest.RoleId;
                await _unitOfWork.UserRoleUOW.AddAsync(userRole);
                var saveChange1 = await _unitOfWork.SaveChangesAsync();
                if (saveChange1 > 0)
                {
                    string emailContent = $"Xin chào {userRequest.UserName},<br><br>"
                                          + $"Tài khoản của bạn đã được tạo thành công.<br>"
                                          + $"Tên đăng nhập: <b>{userRequest.Email}</b><br>"
                                          + $"Mật khẩu: <b>{plainPassword}</b><br><br>"
                                          + $"Vui lòng đổi mật khẩu sau khi đăng nhập.";
                    bool emailSent = false;
                    int retryCount = 0;
                    int maxRetries = 3;
                    
                    //var emailResponse = await _emailService.SendEmail(userRequest.Email, "Tạo tài khoản thành công", emailContent);
                    
                    while (!emailSent && retryCount < maxRetries)
                    {
                        var emailResponse = await _emailService.SendEmail(userRequest.Email, "Tạo tài khoản thành công", emailContent);
                        if (emailResponse.StatusCode == (int)HttpStatusCode.Created)
                        {
                            emailSent = true;
                        }
                        else
                        {
                            retryCount++;
                            await Task.Delay(2000); // Chờ 2s trước khi thử lại
                        }
                    }

                    if (!emailSent)
                    {
                        return ResponseUtil.Error(ResponseMessages.FailedToSendEmail, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
                    }
                    
                    var result = _mapper.Map<UserDto>(user);
                    return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);

                    
                    
                }
                else
                {
                    return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
            }
            
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public static string GenerateRandomString(int length)
    {
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        const string specialCharacters = "!@#$%^&*";

        string allCharacters = lowerCase + upperCase + numbers + specialCharacters;

        Random random = new Random();
        char[] randomString = new char[length];

        for (int i = 0; i < length; i++)
        {
            randomString[i] = allCharacters[random.Next(allCharacters.Length)];
        }

        return new string(randomString);
    }
    
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
    
    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        return Regex.IsMatch(phoneNumber, @"^0\d{9}$");
    }
    
    
    public async Task<ResponseDto> GetProfileAsync(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return ResponseUtil.Error(ResponseMessages.UserIdInvalid, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            User? user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
            {
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }

            if (user.IsDeleted)
            {
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            var result = _mapper.Map<UserDto>(user);
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetAllUserAsync(int page, int limit)
    {
        try
        {
            IEnumerable<User> users = await _unitOfWork.UserUOW.FindAllUserAsync();
            IEnumerable<UserDto> userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            int totalRecords = userDtos.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / limit);

            List<UserDto> result = userDtos.Skip((page - 1) * limit).Take(limit).ToList();
            
            return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> UpdateUserActiveOrDeleteAsync(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return ResponseUtil.Error(ResponseMessages.UserIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }

            User? user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
            {
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            }

            if (user.IsDeleted)
            {
                user.IsDeleted = false;
                await _unitOfWork.UserUOW.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.GetObject(ResponseMessages.UserActive, ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
            }
            else
            {
                user.IsDeleted = true;
                await _unitOfWork.UserUOW.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.GetObject(ResponseMessages.UserHasDeleted, ResponseMessages.DeleteSuccessfully, HttpStatusCode.OK, 1);
            }
            
            
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message, HttpStatusCode.InternalServerError);
        }
    }
    

    
}