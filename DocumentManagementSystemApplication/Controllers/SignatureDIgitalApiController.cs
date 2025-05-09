using DataAccess.DTO;
using DataAccess.DTO.Request;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SignatureDIgitalApiController : ControllerBase
    {
        private readonly ISignApiService _signApiService;

        public SignatureDIgitalApiController(ISignApiService signApiService)
        {
            _signApiService = signApiService;
        }
        
        [HttpPost("create-sign-in-signature-digital")]
        //[AuthorizeResource("[Signaturedigitalapi] Create Sign In Signature Digital")]
        public async Task<ResponseDto> SignInSignatureDigital([FromBody] SignInSignature signInSignature)
        {
            var id = User.FindFirst("userid")?.Value;
            return await _signApiService.SignInSignature(Guid.Parse(id),signInSignature);
        }
        
        [HttpPost("create-signature-digital")]
        //[AuthorizeResource("[Signaturedigitalapi] Create Signature Digital")]
        public async Task<ResponseDto> SignatureDigital([FromBody] SignRequest signRequest)
        {
            var id = User.FindFirst("userid")?.Value;
            return await _signApiService.SignatureApi(Guid.Parse(id),signRequest);
        }
        
    }
}
