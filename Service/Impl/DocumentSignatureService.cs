using System.Net;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Response;
using Repository;
using Service.Utilities;

namespace Service.Impl;

public class DocumentSignatureService : IDocumentSignatureService
{
    private readonly IUnitOfWork _unitOfWork;

    public DocumentSignatureService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto> CreateSignature(Document document, DigitalCertificate digitalCertificate,MetaDataDocument meta, Guid userId,int index)
    {
        var docSig = new DocumentSignature()
        {
            SignedAt = meta.SingingDate,
            //SignatureValue = meta.SerialNumber,
            //ValidFrom = meta.SingingDate,
            OrderIndex = index,
            DocumentId = document.DocumentId,
            DocumentSignatureId=digitalCertificate.DigitalCertificateId
        };
        await _unitOfWork.DocumentSignatureUOW.AddAsync(docSig);
        try
        {
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(docSig, "Signature created successfully", HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error("Error creating signature", ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}