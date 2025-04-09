using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Response;

namespace Service;

public interface IDocumentSignatureService
{
    Task<ResponseDto> CreateSignature(Document document,DigitalCertificate digitalCertificate,MetaDataDocument meta,Guid userId,int index);
}