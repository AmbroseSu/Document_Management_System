using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Repository;
using Service.Response;
using Service.Utilities;
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
            if (string.IsNullOrWhiteSpace(roleDto.RoleName))
            {
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            Role? roleCheck = await _unitOfWork.RoleUOW.FindRoleByNameAsync(roleDto.RoleName);
            if (roleCheck != null)
            {
                return ResponseUtil.Error(ResponseMessages.RoleAlreadyExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }

            Role roleNew = _mapper.Map<Role>(roleDto);
            roleNew.CreatedDate = DateTime.Now;
            await _unitOfWork.RoleUOW.AddAsync(roleNew); 
            await _unitOfWork.SaveChangesAsync();
            var result = _mapper.Map<RoleDto>(roleNew);
            return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, e.Message, HttpStatusCode.InternalServerError);
        }
    }
}