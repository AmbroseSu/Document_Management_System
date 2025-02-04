using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Repository;
using Service.Response;

namespace Service.Impl;

public class RolePermissionService : IRolePermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RolePermissionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseDto> CreateRoleWithPermission(RolePermissionDto rolePermissionDto)
    {
        try
        {
            if (rolePermissionDto.PermissionId == null || rolePermissionDto.RoleId == null)
            {
                return ResponseUtil.Error(ResponseMessages.ValueNull, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            Role? roleCheck = _unitOfWork.RoleUOW.FindRoleByIdAsync(rolePermissionDto.RoleId).Result;
            Permission? permissionCheck = _unitOfWork.PermissionUOW.FindPermissionByIdAsync(rolePermissionDto.PermissionId).Result;
            if (roleCheck == null || permissionCheck == null)
            {
                return ResponseUtil.Error(ResponseMessages.PermissionNotExists + " Or " + ResponseMessages.RoleNotExists, ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
            RolePermission rolePermissionNew = _mapper.Map<RolePermission>(rolePermissionDto);
            await _unitOfWork.RolePermissionUOW.AddAsync(rolePermissionNew);
            var saveChange =  await _unitOfWork.SaveChangesAsync();
            if (saveChange > 0)
            {
                var result = _mapper.Map<RolePermissionDto>(rolePermissionNew);
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