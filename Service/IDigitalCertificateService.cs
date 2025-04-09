using DataAccess.DTO;
using DataAccess.DTO.Response;
using Microsoft.AspNetCore.Http;

namespace Service;

public interface IDigitalCertificateService
{
    Task<ResponseDto> CreateCertificate(MetaDataDocument metaData, Guid userId);
    
}