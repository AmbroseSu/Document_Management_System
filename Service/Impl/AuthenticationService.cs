using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtService = jwtService;
    }


    public async Task<ResponseDto> SignIn(SignInRequest signInRequest)
    {
        try
        {
            /*if (!IsValidEmail(signInRequest.Email))
            {
                return ResponseUtil.Error("Invalid email format", "Failed", HttpStatusCode.BadRequest);
            }*/
            User? user = await _unitOfWork.UserUOW.FindUserByEmail(signInRequest.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.Password))
            {
                return ResponseUtil.Error(ResponseMessages.EmailNotExists + " or " + ResponseMessages.PasswordNotExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            if (user.IsDeleted == true)
            {
                return ResponseUtil.Error(ResponseMessages.UserDelete, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            if (!signInRequest.FcmToken.Equals(user.FcmToken) && !signInRequest.FcmToken.Equals("string"))
            {
                user.FcmToken = signInRequest.FcmToken;
                //await _UnitOfWork.UserUOW.(user);
            }
            
            var userRoles = await _unitOfWork.UserRoleUOW.FindRolesByUserIdAsync(user.UserId);
            var roleNames = new List<string>();
            var resources = new List<Resource>();
            foreach (var userRole in userRoles)
            {
                var roleResources = await _unitOfWork.RoleResourceUOW.FindRoleResourcesByRoleIdAsync(userRole.RoleId);
                var role = await _unitOfWork.RoleUOW.FindRoleByIdAsync(userRole.RoleId);
                roleNames.Add(role.RoleName);
                foreach (var roleResource in roleResources)
                {
                    var resource = await _unitOfWork.ResourceUOW.FindResourceByIdAsync(roleResource.ResourceId);
                    if (resource != null)
                    {
                        resources.Add(resource);
                    }
                }
            }

            var resourceNames = resources.Select(r => r.ResourceName).Distinct().ToList();
            

            var jwt = _jwtService.GenerateToken(user, roleNames, resourceNames);
            var refreshToken = _jwtService.GenerateRefreshToken(user, new Dictionary<string, object>());
                
            JwtAuthenticationResponse jwtAuthResponse = new JwtAuthenticationResponse();
            UserDto userDto = _mapper.Map<UserDto>(user);
                
            jwtAuthResponse.UserDto = userDto;
            jwtAuthResponse.Token = jwt;
            jwtAuthResponse.RefreshToken = refreshToken;
                
            return ResponseUtil.GetObject(jwtAuthResponse, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 0);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ex.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
}