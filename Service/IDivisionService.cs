using DataAccess.DTO;

namespace Service;

public interface IDivisionService
{
    Task<ResponseDto> AddDivisionAsync(string? divisionName,Guid userId);
    Task<ResponseDto> UpdateDivisionAsync(DivisionDto divisionDto,Guid userId);
    Task<ResponseDto> GetAllDivisionAsync(string? divisionName, int page, int limit);
    Task<ResponseDto> UpdateDivisionActiveOrDeleteAsync(Guid divisionId,Guid userId);
    Task<ResponseDto> GetDivisionDetails(Guid divisionId);
}