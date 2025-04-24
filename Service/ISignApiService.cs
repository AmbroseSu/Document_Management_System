using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface ISignApiService
{
    Task<ResponseDto> SignInSignature(Guid userId, SignInSignature signInSignature);
    Task<ResponseDto> SignatureApi(Guid userId, SignRequest signRequest);
}