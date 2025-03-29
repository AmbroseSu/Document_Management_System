using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class DivisionService : IDivisionService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public DivisionService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResponseDto> AddDivisionAsync(string? divisionName)
    {
        try
        {
            if (divisionName == null)
            {
                return ResponseUtil.Error(ResponseMessages.DivisionNameNotNull, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            
            var divisionWithName = await _unitOfWork.DivisionUOW.FindDivisionByNameAsync(divisionName);
            if (divisionWithName != null)
            {
                return ResponseUtil.Error(ResponseMessages.DivisionNameExisted, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            var division = new Division
            {
                DivisionName = divisionName,
                IsDeleted = false
            };
            //var division = _mapper.Map<Division>(divisionDto);
            await _unitOfWork.DivisionUOW.AddAsync(division);
            await _unitOfWork.SaveChangesAsync();
            var result = _mapper.Map<DivisionDto>(division);
            return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> UpdateDivisionAsync(DivisionDto divisionDto)
    {
        try
        {
            if (divisionDto.DivisionName == null)
            {
                return ResponseUtil.Error(ResponseMessages.DivisionNotChanged, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            var division = await _unitOfWork.DivisionUOW.FindDivisionByIdAsync(divisionDto.DivisionId);
            if (division == null)
            {
                return ResponseUtil.Error(ResponseMessages.DivisionNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            }
            if (division.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.DivisionHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            
            var divisionWithName = await _unitOfWork.DivisionUOW.FindDivisionByNameAsync(divisionDto.DivisionName);
            if (divisionWithName != null)
            {
                return ResponseUtil.Error(ResponseMessages.DivisionNameExisted, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }

            division.DivisionName = divisionDto.DivisionName;
            await _unitOfWork.DivisionUOW.UpdateAsync(division);
            await _unitOfWork.SaveChangesAsync();
            var result = _mapper.Map<DivisionDto>(division);
            return ResponseUtil.GetObject(result, ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetAllDivisionAsync(int page, int limit)
    {
        try
        {
            var divisions = await _unitOfWork.DivisionUOW.FindAllDivisionAsync();
            
            var totalRecords = divisions.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / limit);

            IEnumerable<Division> divisionResults = divisions
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            var result = _mapper.Map<IEnumerable<DivisionDto>>(divisionResults);
            return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    
    public async Task<ResponseDto> UpdateDivisionActiveOrDeleteAsync(Guid divisionId)
    {
        try
        {
            if (divisionId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.DivisionIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var division = await _unitOfWork.DivisionUOW.FindDivisionByIdAsync(divisionId);
            if (division == null)
                return ResponseUtil.Error(ResponseMessages.DivisionNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            

            if (division.IsDeleted)
            {
                division.IsDeleted = false;
                await _unitOfWork.DivisionUOW.UpdateAsync(division);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.GetObject(ResponseMessages.DivisionActive, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 1);
            }

            division.IsDeleted = true;
            await _unitOfWork.DivisionUOW.UpdateAsync(division);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.DivisionHasDeleted, ResponseMessages.DeleteSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetDivisionDetails(Guid divisionId)
    {
        try
        {
            if (divisionId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.DivisionIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var division = await _unitOfWork.DivisionUOW.FindDivisionByIdAsync(divisionId);
            if (division == null)
                return ResponseUtil.Error(ResponseMessages.DivisionNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            var result = _mapper.Map<DivisionDto>(division);
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
}