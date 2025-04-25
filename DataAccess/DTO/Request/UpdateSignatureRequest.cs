using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class UpdateSignatureRequest
{
    public IFormFile? NormalSignature { get; set; }
    public IFormFile? DigitalSignature { get; set; }
}