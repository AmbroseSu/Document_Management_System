using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service;

public interface IUserDocPermissionService
{
    Task<ResponseDto> GrantPermissionForDocument(Guid userGrantId, GrantDocumentRequest grantDocumentRequest);
}