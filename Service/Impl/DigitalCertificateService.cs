using System.Net;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Response;
using Repository;
using Service.Utilities;

namespace Service.Impl;

public class DigitalCertificateService : IDigitalCertificateService
{
    private readonly IUnitOfWork _unitOfWork;

    public DigitalCertificateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto> CreateCertificate(MetaDataDocument meta, Guid userId)
    {
        var certificate = new DigitalCertificate()
        {
            SerialNumber = meta.SerialNumber,
            Issuer = meta.SignerName,
            ValidFrom = meta.ValidFrom,
            ValidTo = meta.ExpirationDate,
            //HashAlgorithm = meta.Algorithm,
            //PublicKey = "PublicKey",
            //IsRevoked = false,
            SignatureImageUrl = "SignatureImageUrl",
            UserId = userId,
            //OwnerName = meta.SignerName
        };
        await _unitOfWork.DigitalCertificateUOW.AddAsync(certificate);
        try
        {
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(certificate, "Certificate created successfully", HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error("Error creating certificate",ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}