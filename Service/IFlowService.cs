using BusinessObject.Enums;
using DataAccess.DTO;

namespace Service;

public interface IFlowService
{
    Task<ResponseDto> FindAllFlowAsync(Scope scope);
}