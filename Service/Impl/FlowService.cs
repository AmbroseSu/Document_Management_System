using System.Net;
using BusinessObject.Enums;
using DataAccess.DTO;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class FlowService : IFlowService
{
    private readonly IUnitOfWork _unitOfWork;

    public FlowService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto> FindAllFlowAsync(Scope scope)
    {
        try
        {
            var flows = await _unitOfWork.FlowUOW.FindAllFlowAsync(scope);
            
            return ResponseUtil.GetObject(flows, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, flows.Count());
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
}