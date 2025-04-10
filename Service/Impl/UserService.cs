using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class UserService : IUserService
{
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork,
        IEmailService emailService)
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
            if (userRequest == null) throw new ArgumentNullException(nameof(userRequest));

            var userExistEmail = await _unitOfWork.UserUOW.FindUserByEmailAsync(userRequest.Email);
            if (userExistEmail != null)
                return ResponseUtil.Error(ResponseMessages.EmailAlreadyExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            /*if (userExistEmail!.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);*/
            //var emailAttribute = new EmailAddressAttribute();
            /*if (IsValidEmail(userRequest.Email))
            {
                return ResponseUtil.Error(ResponseMessages.InvalidEmailFormat, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }*/
            var userExistUserName = await _unitOfWork.UserUOW.FindUserByUserNameAsync(userRequest.UserName);
            if (userExistUserName != null)
                return ResponseUtil.Error(ResponseMessages.UserNameAlreadyExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            /*if (userExistUserName!.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);*/

            if (!IsValidEmail(userRequest.Email))
                return ResponseUtil.Error(ResponseMessages.EmailFormatInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            if (!IsValidPhoneNumber(userRequest.PhoneNumber))
                return ResponseUtil.Error(ResponseMessages.PhoneNumberFormatInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var plainPassword = GenerateRandomString(10);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);


            var user = _mapper.Map<User>(userRequest);
            //user.Password = BCrypt.Net.BCrypt.HashPassword("Hieu1234");
            user.Password = hashedPassword;
            user.PhoneNumber = userRequest.PhoneNumber;
            user.DateOfBirth = userRequest.DateOfBirth;
            user.FullName = userRequest.FullName;
            user.Position = userRequest.Position;
            user.IdentityCard = userRequest.IdentityCard;
            user.IsDeleted = false;
            user.IsEnable = false;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.UserUOW.AddAsync(user);
            var saveChange = await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                var userRole = new UserRole();
                userRole.UserId = user.UserId;
                userRole.RoleId = userRequest.RoleId;
                await _unitOfWork.UserRoleUOW.AddAsync(userRole);
                var saveChange1 = await _unitOfWork.SaveChangesAsync();
                if (saveChange1 > 0)
                {
                    var emailContent = $"Xin chào {userRequest.UserName},<br><br>"
                                       + $"Tài khoản của bạn đã được tạo thành công.<br>"
                                       + $"Tên đăng nhập: <b>{userRequest.Email}</b><br>"
                                       + $"Mật khẩu: <b>{plainPassword}</b><br><br>"
                                       + $"Vui lòng đổi mật khẩu sau khi đăng nhập.";
                    var emailSent = false;
                    var retryCount = 0;
                    var maxRetries = 3;

                    //var emailResponse = await _emailService.SendEmail(userRequest.Email, "Tạo tài khoản thành công", emailContent);

                    while (!emailSent && retryCount < maxRetries)
                    {
                        var emailResponse = await _emailService.SendEmail(userRequest.Email, "Tạo tài khoản thành công",
                            emailContent);
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
                        return ResponseUtil.Error(ResponseMessages.FailedToSendEmail, ResponseMessages.OperationFailed,
                            HttpStatusCode.InternalServerError);

                    var result = _mapper.Map<UserDto>(user);
                    return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created,
                        1);
                }

                return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed,
                    HttpStatusCode.InternalServerError);
            }

            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ResponseDto> GetProfileAsync(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.UserIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            
            var userRoles = await _unitOfWork.UserRoleUOW.FindRolesByUserIdAsync(user.UserId);
            var roles = new List<RoleDto>();
            foreach (var userRole in userRoles)
            {
                var roleResources = await _unitOfWork.RoleResourceUOW.FindRoleResourcesByRoleIdAsync(userRole.RoleId);
                var role = await _unitOfWork.RoleUOW.FindRoleByIdAsync(userRole.RoleId);
                var roleDto = _mapper.Map<RoleDto>(role);
                roles.Add(roleDto);
            }
            
            var result = _mapper.Map<UserDto>(user);
            result.Roles = roles;
            var division = await _unitOfWork.DivisionUOW.FindDivisionByIdAsync(user.DivisionId);
            if (division != null)
            {
                var divisionDto = _mapper.Map<DivisionDto>(division);
                result.DivisionDto = divisionDto;
            }
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }

    /*public async Task<ResponseDto> GetAllUserAsync(int page, int limit)
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
    }*/

    public async Task<ResponseDto> GetAllUserAsync(UserFilterRequest userFilterRequest)
    {
        try
        {
            var users = await _unitOfWork.UserUOW.FindAllUserAsync();
            var query = users.AsQueryable();
            if (!string.IsNullOrEmpty(userFilterRequest.Filters.FullName))
                query = query.Where(u => u.FullName!.ToLower().Contains(userFilterRequest.Filters.FullName.ToLower()));

            if (!string.IsNullOrEmpty(userFilterRequest.Filters.Email))
                query = query.Where(u => u.Email!.ToLower().Contains(userFilterRequest.Filters.Email.ToLower()));
            if (!string.IsNullOrEmpty(userFilterRequest.Filters.Division))
                query = query.Include(u => u.Division).Where(u =>
                    u.Division != null && u.Division.DivisionName.ToLower()
                        .Contains(userFilterRequest.Filters.Division.ToLower()));

            if (!string.IsNullOrEmpty(userFilterRequest.Filters.Position))
                query = query.Where(u => u.Position!.ToLower().Contains(userFilterRequest.Filters.Position.ToLower()));
            if (!string.IsNullOrEmpty(userFilterRequest.Filters.Role))
                query = query.Where(u =>
                    u.UserRoles!.Any(ur => ur.Role!.RoleName.Contains(userFilterRequest.Filters.Role)));
            if (userFilterRequest.Sort != null)
            {
                var isDescending = userFilterRequest.Sort.Order.ToLower() == "desc";

                // Dùng Reflection để sort theo tên field
                query = isDescending
                    ? query.OrderByDescending(u => GetPropertyValue(u, userFilterRequest.Sort.Field))
                    : query.OrderBy(u => GetPropertyValue(u, userFilterRequest.Sort.Field));
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }

            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / userFilterRequest.Limit);

            IEnumerable<User> userResults = query
                .Skip((userFilterRequest.Page - 1) * userFilterRequest.Limit)
                .Take(userFilterRequest.Limit)
                .ToList();
            var result = _mapper.Map<IEnumerable<UserDto>>(userResults);

            return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords,
                userFilterRequest.Page, userFilterRequest.Limit, totalPages);
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
                return ResponseUtil.Error(ResponseMessages.UserIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            

            if (user.IsDeleted)
            {
                user.IsDeleted = false;
                await _unitOfWork.UserUOW.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.GetObject(ResponseMessages.UserActive, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 1);
            }

            user.IsDeleted = true;
            await _unitOfWork.UserUOW.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.UserHasDeleted, ResponseMessages.DeleteSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ResponseDto> UpdateUserAsync(UserUpdateRequest userUpdateRequest)
    {
        try
        {
            if (userUpdateRequest.UserId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.UserIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userUpdateRequest.UserId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);


            var hasChanges = false; // Biến kiểm tra có thay đổi gì không

            // Cập nhật từng thuộc tính nếu hợp lệ
            if (!string.IsNullOrWhiteSpace(userUpdateRequest.Address) && user.Address != userUpdateRequest.Address)
            {
                user.Address = userUpdateRequest.Address;
                hasChanges = true;
            }

            if (userUpdateRequest.DateOfBirth.HasValue && user.DateOfBirth != userUpdateRequest.DateOfBirth)
            {
                user.DateOfBirth = userUpdateRequest.DateOfBirth.Value;
                hasChanges = true;
            }

            if (userUpdateRequest.Gender.HasValue && user.Gender != userUpdateRequest.Gender)
            {
                user.Gender = userUpdateRequest.Gender.Value;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateRequest.Avatar) && user.Avatar != userUpdateRequest.Avatar)
            {
                user.Avatar = userUpdateRequest.Avatar;
                hasChanges = true;
            }

            if (!hasChanges)
                return ResponseUtil.GetObject(ResponseMessages.NoChangesDetected, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 0);

            await _unitOfWork.UserUOW.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.UserHasUpdatedInformation,
                ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ResponseDto> AdminUpdateUserAsync(AdminUpdateUserRequest adminUpdateUserRequest)
    {
        try
        {
            if (adminUpdateUserRequest.UserId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.UserIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(adminUpdateUserRequest.UserId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.UserNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var hasChanges = false;

            // Cập nhật từng thuộc tính nếu hợp lệ
            if (!string.IsNullOrWhiteSpace(adminUpdateUserRequest.FullName) &&
                user.FullName != adminUpdateUserRequest.FullName)
            {
                user.FullName = adminUpdateUserRequest.FullName;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(adminUpdateUserRequest.Email) && user.Email != adminUpdateUserRequest.Email)
            {
                user.Email = adminUpdateUserRequest.Email;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(adminUpdateUserRequest.Address) &&
                user.Address != adminUpdateUserRequest.Address)
            {
                user.Address = adminUpdateUserRequest.Address;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(adminUpdateUserRequest.PhoneNumber) &&
                user.PhoneNumber != adminUpdateUserRequest.PhoneNumber)
            {
                user.PhoneNumber = adminUpdateUserRequest.PhoneNumber;
                hasChanges = true;
            }

            if (adminUpdateUserRequest.Gender.HasValue && user.Gender != adminUpdateUserRequest.Gender)
            {
                user.Gender = adminUpdateUserRequest.Gender.Value;
                hasChanges = true;
            }

            if (adminUpdateUserRequest.DateOfBirth.HasValue && user.DateOfBirth != adminUpdateUserRequest.DateOfBirth)
            {
                user.DateOfBirth = adminUpdateUserRequest.DateOfBirth.Value;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(adminUpdateUserRequest.Position) &&
                user.Position != adminUpdateUserRequest.Position)
            {
                user.Position = adminUpdateUserRequest.Position;
                hasChanges = true;
            }

            if (adminUpdateUserRequest.DivisionId.HasValue && user.DivisionId != adminUpdateUserRequest.DivisionId)
            {
                user.DivisionId = adminUpdateUserRequest.DivisionId.Value;
                hasChanges = true;
            }


            if (!string.IsNullOrWhiteSpace(adminUpdateUserRequest.Avatar) &&
                user.Avatar != adminUpdateUserRequest.Avatar)
            {
                user.Avatar = adminUpdateUserRequest.Avatar;
                hasChanges = true;
            }


            if (!hasChanges)
                return ResponseUtil.GetObject(ResponseMessages.NoChangesDetected, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 0);

            await _unitOfWork.UserUOW.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ResponseUtil.GetObject(ResponseMessages.UserHasUpdatedInformation,
                ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public static string GenerateRandomString(int length)
    {
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        const string specialCharacters = "!@#$%^&*";

        var allCharacters = lowerCase + upperCase + numbers + specialCharacters;

        var random = new Random();
        var randomString = new char[length];

        for (var i = 0; i < length; i++) randomString[i] = allCharacters[random.Next(allCharacters.Length)];

        return new string(randomString);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        return Regex.IsMatch(phoneNumber, @"^0\d{9}$");
    }

    private object GetPropertyValue(User user, string propertyName)
    {
        var property = typeof(User).GetProperty(propertyName);
        return property?.GetValue(user);
    }
}