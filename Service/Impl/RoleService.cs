using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Repository;
using Service.Response;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    public async Task SeedRolesAsync()
    {
        var defaultRole = new List<Role>
        {
            new Role { RoleName = "Admin", CreatedDate = null },
            new Role { RoleName = "Leader", CreatedDate = null },
            new Role { RoleName = "Division Head", CreatedDate = null },
            new Role { RoleName = "Specialist", CreatedDate = null },
            new Role { RoleName = "Chief", CreatedDate = null },
            new Role { RoleName = "Clerical Assistant", CreatedDate = null },
        };
        
        // Lấy danh sách Role hiện có từ DB qua unitOfWork
        var existingRoles = await _unitOfWork.RoleUOW.GetAllAsync();
        
        // Lọc ra các role mới chưa có trong DB
        var newRoles = defaultRole
            .Where(dr => !existingRoles.Any(er => er.RoleName == dr.RoleName))
            .ToList();
        
        if (newRoles.Any())
        {
            // Thêm các quyền mới vào DB
            await _unitOfWork.RoleUOW.AddRangeAsync(newRoles);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<ResponseDto> CreateRole(RoleDto roleDto)
    {
        try
        {
            if (roleDto.RoleName == null)
            {
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            Role? roleCheck = _unitOfWork.RoleUOW.FindRoleByNameAsync(roleDto.RoleName).Result;
            if (roleCheck != null)
            {
                return ResponseUtil.Error(ResponseMessages.RoleAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            Role roleNew = _mapper.Map<Role>(roleDto);
            await _unitOfWork.RoleUOW.AddAsync(roleNew);
            var saveChange =  await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                var result = _mapper.Map<RoleDto>(roleNew);
                return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
            }
            else
            {
                return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
            }


        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
}