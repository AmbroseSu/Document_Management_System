using DataAccess.DTO;

namespace Service;

public interface IDivisionService
{
    Task<ResponseDto> AddDivisionAsync(string? divisionName);
    Task<ResponseDto> UpdateDivisionAsync(DivisionDto divisionDto);
    Task<ResponseDto> GetAllDivisionAsync(string? divisionName, int page, int limit);
    Task<ResponseDto> UpdateDivisionActiveOrDeleteAsync(Guid divisionId);
    Task<ResponseDto> GetDivisionDetails(Guid divisionId);
}