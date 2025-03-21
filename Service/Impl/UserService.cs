using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using AutoMapper;
using BusinessObject;
using DataAccess.DAO;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Npgsql.Replication;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }


    public async Task<ResponseDto> CreateUserByForm(UserRequest userRequest)
    {
        try
        {
            if (userRequest == null)
            {
                throw new ArgumentNullException(nameof(userRequest));
            }
            
            User? userExistEmail = await _unitOfWork.UserUOW.FindUserByEmail(userRequest.Email);
            if (userExistEmail != null)
            {
                return ResponseUtil.Error(ResponseMessages.EmailAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            //var emailAttribute = new EmailAddressAttribute();
            /*if (IsValidEmail(userRequest.Email))
            {
                return ResponseUtil.Error(ResponseMessages.InvalidEmailFormat, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }*/
            User? userExistUserName = await _unitOfWork.UserUOW.FindUserByUserName(userRequest.UserName);
            if (userExistUserName != null)
            {
                return ResponseUtil.Error(ResponseMessages.UserNameAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            User user = _mapper.Map<User>(userRequest);
            user.Password = BCrypt.Net.BCrypt.HashPassword("Hieu1234");
            user.PhoneNumber = "0123456789";
            user.IsDeleted = false;
            user.IsEnable = false;
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
    
    public bool IsValidEmail(string email)
    {
        try
        {
            var mail = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}