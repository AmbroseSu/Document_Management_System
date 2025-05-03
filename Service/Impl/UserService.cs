using System.Globalization;
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
// using Task = System.Threading.Tasks.Task;
using System.Linq.Dynamic.Core;
using System.Text;
using BusinessObject.Enums;
using BusinessObject.Option;
using ClosedXML.Excel;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Syncfusion.OfficeChart.Implementation;


namespace Service.Impl;

public class UserService : IUserService
{   
    private readonly IFileService _fileService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly string _host;
    public UserService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork,
        IEmailService emailService, IFileService fileService,
        IOptions<AppsetingOptions> options)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _fileService = fileService;
        _host = options.Value.Host;
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
                userRole.IsPrimary = true;
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
            var listDigitalCer = await _unitOfWork.DigitalCertificateUOW.FindDigitalCertificateByUserIdAsync(userId) 
                                 ?? new List<DigitalCertificate>();

            result.Sign = listDigitalCer
                .FirstOrDefault(dice => dice.IsUsb == null)?.SignatureImageUrl;

            result.SignDigital = listDigitalCer
                .FirstOrDefault(dice => dice.IsUsb != null)?.SignatureImageUrl;
            
            var division = await _unitOfWork.DivisionUOW.FindDivisionByIdAsync(user.DivisionId);
            if (division != null)
            {
                var divisionDto = _mapper.Map<DivisionDto>(division);
                result.DivisionDto = divisionDto;
            }
            
            var listDocuments = await _unitOfWork.DocumentUOW.FindDocumentByUserIdAsync(userId);
            var listArchiveDocuments = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByUserIdAsync(userId);
            result.NumberOfDocuments = listDocuments.Count();
            result.NumberOfArchiveDocuments = listArchiveDocuments.Count();
            
            
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
                    u.UserRoles!.Any(ur => ur.Role!.RoleName.ToLower().Contains(userFilterRequest.Filters.Role.ToLower()) && ur.IsPrimary == true));
            if (!string.IsNullOrEmpty(userFilterRequest.Sort?.Field))
            {
                var sortField = userFilterRequest.Sort.Field;
                var sortOrder = userFilterRequest.Sort.Order?.ToLower() == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{sortField} {sortOrder}");
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
            
            
            var userIds = userResults.Select(u => u.UserId).ToList();
            var divisionIds = userResults.Where(u => u.DivisionId != null).Select(u => u.DivisionId!.Value).Distinct().ToList();

            // 1. Lấy tất cả UserRoles
            var userRoles = await _unitOfWork.UserRoleUOW.FindUserRolesMainByUserIdsAsync(userIds);

            // 2. Lấy tất cả Roles
            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
            var roles = await _unitOfWork.RoleUOW.FindRolesByIdsAsync(roleIds);
            var roleMap = roles.ToDictionary(r => r.RoleId, r => _mapper.Map<RoleDto>(r));

            // 3. Lấy tất cả Divisions
            var divisions = await _unitOfWork.DivisionUOW.FindDivisionsByIdsAsync(divisionIds);
            var divisionMap = divisions.ToDictionary(d => d.DivisionId, d => _mapper.Map<DivisionDto>(d));

            // 4. Map vào kết quả
            var result = _mapper.Map<List<UserDto>>(userResults);
            foreach (var userDto in result)
            {
                var ur = userRoles.Where(ur => ur.UserId == userDto.UserId).ToList();
                userDto.Roles = ur.Select(r => roleMap[r.RoleId]).ToList();

                if (userDto.DivisionId.HasValue && divisionMap.TryGetValue(userDto.DivisionId.Value, out var divDto))
                {
                    userDto.DivisionDto = divDto;
                }
                var listDigitalCer = await _unitOfWork.DigitalCertificateUOW.FindDigitalCertificateByUserIdAsync(userDto.UserId) 
                                     ?? new List<DigitalCertificate>();

                userDto.Sign = listDigitalCer
                    .FirstOrDefault(dice => dice.IsUsb == null)?.SignatureImageUrl;

                userDto.SignDigital = listDigitalCer
                    .FirstOrDefault(dice => dice.IsUsb != null)?.SignatureImageUrl;
            }
            

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
            
            var userRoles = user.UserRoles
                .Select(ur => ur.Role)
                .ToList();
            
            var matchedRoles = userRoles.Where(role => role.RoleId == adminUpdateUserRequest.SubRoleId).ToList();
            
            if (adminUpdateUserRequest.SubRoleId.HasValue && !matchedRoles.Any())
            {
                
                if (userRoles.Count >= 2 && userRoles.Any(r => r.RoleName.Contains("_")))
                {
                    var rolesToRemove = user.UserRoles
                        .Where(ur => ur.Role.RoleName.Contains("_"))
                        .ToList();

                    foreach (var ur in rolesToRemove)
                    {
                        _unitOfWork.UserRoleUOW.DeleteAsync(ur);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
                
                var userRole = new UserRole();
                userRole.UserId = user.UserId;
                userRole.RoleId = adminUpdateUserRequest.SubRoleId.Value;
                userRole.IsPrimary = false;
                await _unitOfWork.UserRoleUOW.AddAsync(userRole);
                await _unitOfWork.SaveChangesAsync();
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
    
    
    public async Task<ResponseDto> ImportUsersFromFileAsync(List<FileImportData> fileImportDatas, Guid divisionId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            

        //OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        

        foreach (var row in fileImportDatas)
        {
            Guid roleId = await GetRoleIdByName(row.RoleName!); // Bạn có thể dùng repo lấy role theo tên

            var userRequest = new UserRequest
            {
                FullName = row.FullName!,
                UserName = row.UserName!,
                Email = row.Email!,
                PhoneNumber = row.PhoneNumber!,
                IdentityCard = row.IdentityCard!,
                //DateOfBirth = dob,
                Address = row.Address!,
                Gender = row.Gender,
                Position = row.Position!,
                RoleId = roleId,
                DivisionId = divisionId
            };
            var result = await CreateUserByForm(userRequest);
            if (result.StatusCode != (int)HttpStatusCode.Created)
            {
                await transaction.RollbackAsync();
                return ResponseUtil.Error($"Dòng {row}: {result.Message}", ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            }
        }
        
        await transaction.CommitAsync();
        return ResponseUtil.GetObject(ResponseMessages.ImportSuccessfully, ResponseMessages.CreatedSuccessfully,
            HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
        
    }
    
    public async Task<List<FileImportData>> ReadUsersFromExcelAsync(IFormFile file)
    {
        var users = new List<FileImportData>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1); // Sheet đầu tiên
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Bỏ dòng tiêu đề

        foreach (var row in rows)
        {
            var fullName = row.Cell(2).GetValue<string>().Trim();
            var userName = row.Cell(3).GetValue<string>().Trim();
            var email = row.Cell(4).GetValue<string>().Trim();
            var phone = row.Cell(5).GetValue<string>().Trim();
            var idCard = row.Cell(6).GetValue<string>().Trim();
            var address = row.Cell(7).GetValue<string>().Trim();
            var genderRaw = row.Cell(8).GetValue<string>().Trim().ToLower();
            var position = row.Cell(9).GetValue<string>().Trim();
            var roleName = row.Cell(10).GetValue<string>().Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                continue;

            Gender gender = genderRaw switch
            {
                "nam" => Gender.MALE,
                "nữ" or "nu" => Gender.FEMALE,
                _ => Gender.OTHER
            };

            // Nếu bạn không cần lấy roleId ở đây, có thể bỏ qua
            var fileImportData = new FileImportData
            {
                FullName = fullName,
                UserName = userName,
                Email = email,
                PhoneNumber = phone,
                IdentityCard = idCard,
                Address = address,
                Gender = gender,
                Position = position,
                RoleName = roleName
            };

            users.Add(fileImportData);
        }

        return users;
    }
    
    
    public async Task<List<FileImportData>> ReadUsersFromCsvAsync(IFormFile file)
    {
        var users = new List<FileImportData>();

        using var stream = new StreamReader(file.OpenReadStream(), Encoding.UTF8); // Hoặc Encoding.Default nếu bị lỗi
        string? line;
        int rowIndex = 0;

        while ((line = await stream.ReadLineAsync()) != null)
        {
            rowIndex++;

            if (rowIndex == 1) continue; // Bỏ qua dòng tiêu đề

            var columns = line.Split(',');

            if (columns.Length < 10)
            {
                // Thêm thông báo lỗi vào danh sách nếu dòng thiếu dữ liệu
                continue;
            }

            var fullName = columns[1].Trim();
            var userName = columns[2].Trim();
            var email = columns[3].Trim();
            var phone = columns[4].Trim();
            var idCard = columns[5].Trim();
            var address = columns[6].Trim();
            var genderRaw = columns[7].Trim().ToLower();
            var position = columns[8].Trim();
            var roleName = columns[9].Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                continue;

            Gender gender = genderRaw switch
            {
                "nam" => Gender.MALE,
                "nữ" or "nu" => Gender.FEMALE,
                _ => Gender.OTHER
            };

            // Tạo đối tượng FileImportData tương tự như trong code đọc Excel
            var fileImportData = new FileImportData
            {
                FullName = fullName,
                UserName = userName,
                Email = email,
                PhoneNumber = phone,
                IdentityCard = idCard,
                Address = address,
                Gender = gender,
                Position = position,
                RoleName = roleName
            };

            users.Add(fileImportData);
        }

        return users;
    }

    
    
    
    
    

    private async Task<Guid> GetRoleIdByName(string roleName)
    {
        var role = await _unitOfWork.RoleUOW.FindRoleByNameAsync(roleName);
        return role?.RoleId ?? Guid.Empty;
    }
    
    

    public async Task<ResponseDto> UpdateAvatarAsync(IFormFile file,string id)
    {
        var url = _host+"/api/User/view-avatar/"+ await _fileService.SaveAvatar(file, Guid.NewGuid().ToString());
        return ResponseUtil.GetObject(url,"ok",HttpStatusCode.OK,1);
    }

    public async Task<IActionResult> GetAvatar(string userId)
    {
        // var urlImg = (await _unitOfWork.UserUOW.FindUserByIdAsync(Guid.Parse(userId))).Avatar.Split("/")[^1];
        return await _fileService.GetAvatar(userId);
    }

public async Task<ResponseDto> UploadSignatureImgAsync(UpdateSignatureRequest updateSignatureRequest, Guid userId)
    {
        var normalSign = updateSignatureRequest.NormalSignature;
        var digitalSign = updateSignatureRequest.DigitalSignature;

        
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        if(user!.IsEnable) return ResponseUtil.Error("Tài khoản đã cập nhật thông tin lần đâu, vui lòng liên hệ Admin để được cập nhật lại thông tin",
            ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        var listCer = user!.DigitalCertificates;
        
        if (listCer == null || listCer.Count == 0)
        {
            if (normalSign == null)
            {
                return ResponseUtil.Error("Đăng nhập lần đầu, vui lòng upload ảnh chữ ký nháy",
                    ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
            }
            if(!Path.GetExtension(normalSign.FileName).Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                return ResponseUtil.Error("File chữ ký không đúng định dạng, vui lòng chọn lại file",
                    ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
            var cerId = Guid.NewGuid();
            var name = await _fileService.SaveSignature(normalSign, cerId.ToString());
            var url = _host + "/api/User/view-signature-img/" + name;
            var cer = new DigitalCertificate
            {
                DigitalCertificateId = cerId,
                SerialNumber = "DMS" + GenerateRandomString(5),
                Issuer = "Hệ thống",
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddYears(99),
                Subject = user.FullName,
                IsUsb = null,
                SignatureImageUrl = url,
                UserId = user.UserId,
                User = user
            };
            await _unitOfWork.DigitalCertificateUOW.AddAsync(cer);
            // _fileService.InsertTextToImage(name, updateSignatureRequest.Name);
            if (digitalSign != null)
            {
                if(!Path.GetExtension(digitalSign.FileName).Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                    return ResponseUtil.Error("File chữ ký không đúng định dạng, vui lòng chọn lại file",
                        ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
                var cerDigitalId = Guid.NewGuid();
                var nameDigital = await _fileService.SaveSignature(digitalSign, cerDigitalId.ToString());
                var urlDigital = _host + "/api/User/view-signature-img/" + nameDigital;
                var cerDigital = new DigitalCertificate
                {
                    DigitalCertificateId = cerDigitalId,
                    SerialNumber = null,
                    Issuer = null,
                    ValidFrom = DateTime.Now,
                    ValidTo = DateTime.Now.AddYears(1),
                    Subject = user.FullName,
                    IsUsb = false,
                    SignatureImageUrl = urlDigital,
                    UserId = user.UserId,
                    User = user
                };
                await _unitOfWork.DigitalCertificateUOW.AddAsync(cerDigital);
                
            }
            
        }
        else
        {
            if(digitalSign==null && normalSign ==null) return ResponseUtil.Error("Vui lòng upload ít nhất 1 ảnh chữ ký",
                ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
            foreach (var cer in listCer)
            {
                if (digitalSign != null)
                {
                    await _fileService.SaveSignature(digitalSign, cer.DigitalCertificateId.ToString());
                    if(!Path.GetExtension(digitalSign.FileName).Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                        return ResponseUtil.Error("File chữ ký không đúng định dạng, vui lòng chọn lại file",
                            ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
                }

                if (normalSign == null) continue;
                await _fileService.SaveSignature(normalSign, cer.DigitalCertificateId.ToString());
                if(!Path.GetExtension(normalSign.FileName).Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                    return ResponseUtil.Error("File chữ ký không đúng định dạng, vui lòng chọn lại file",
                        ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
            }
        }

        user.IsEnable = true;
        await _unitOfWork.UserUOW.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return ResponseUtil.GetObject("ok", ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
        
    }
    
    public async Task<ResponseDto> EnableUploadSignatureImage(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
                return ResponseUtil.Error(ResponseMessages.EmailNotExists, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            if (user.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.UserHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            user.IsEnable = false;
            await _unitOfWork.UserUOW.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.EnableUploadSignatureImage, ResponseMessages.UpdateSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ResponseDto> FindUserCanGrandPermission(Guid archivedDocId)
    {
        try
        {
            var archivedDocument = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(archivedDocId);
            if (archivedDocument == null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            var existingPermissions = await _unitOfWork.UserDocPermissionUOW.GetPermissionsByDocumentIdAsync(archivedDocId);
            var userIdsAlreadyGranted = existingPermissions
                .Where(p => p.IsDeleted == false)
                .Select(p => p.UserId)
                .ToList();
            var allUsers = await _unitOfWork.UserUOW.FindAllUserAsync();
            var allUserNotDeletes = allUsers.Where(u => u.IsDeleted == false);
            var availableUsers = allUserNotDeletes
                .Where(u => !userIdsAlreadyGranted.Contains(u.UserId))
                .ToList();
            var result = _mapper.Map<List<UserDto>>(availableUsers);
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IActionResult> GetSignatureImg(string userId)
    {
        return await _fileService.GetSignature(userId);
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