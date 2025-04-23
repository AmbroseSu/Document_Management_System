using System.Net;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class SignApiService
{
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;

    public SignApiService(IFileService fileService, IUnitOfWork unitOfWork)
    {
        _fileService = fileService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResponseDto> SignApi(SignRequest signRequest)
    {
        try
        {
            var verificationOtp = await _unitOfWork.VerificationOtpUOW.FindByTokenAsync(signRequest.OtpCode);
            if (verificationOtp == null)
                return ResponseUtil.Error(ResponseMessages.OtpNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            
            
            
            return null;
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.InternalServerError);
        }
    }
}