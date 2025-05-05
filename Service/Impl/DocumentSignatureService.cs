using System.Net;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Response;
using Repository;
using Service.Response;
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
            DocumentVersionId = document.DocumentVersions!.OrderByDescending(x => int.Parse(x.VersionNumber!)).FirstOrDefault()!.DocumentVersionId,
            OrderIndex = index,
            DigitalCertificateId= digitalCertificate.DigitalCertificateId
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

    public async Task<ResponseDto> CreateSignatureApprove(Guid userId, Guid documentId)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        if (user == null)
        {
            return ResponseUtil.Error("User not found", ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }
        if (user.DigitalCertificates == null)
        {
            return ResponseUtil.Error("User does not have a digital certificate", ResponseMessages.FailedToSaveData,
                HttpStatusCode.BadRequest);
        }

        var cer = user.DigitalCertificates.FirstOrDefault(x => x.IsUsb == null);
        if (document == null)
        {
            return ResponseUtil.Error("Document not found", ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }

        if (document.DocumentVersions == null)
        {
            return ResponseUtil.Error("Document versions not found", ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }

        var finalVer = document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion);
        if (finalVer == null)
        {
            return ResponseUtil.Error("Final version not found", ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }

        var listSign = finalVer.DocumentSignatures;
        if (listSign == null)
        {
            return ResponseUtil.Error("Document signatures not found", ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }

        var orderMax = listSign.OrderByDescending(x => x.OrderIndex).FirstOrDefault()!.OrderIndex;
        
        if(cer == null)
        {
            return ResponseUtil.Error("Digital certificate not found", ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }

        var docSign = new DocumentSignature()
        {
            SignedAt = DateTime.UtcNow,
            DocumentVersionId =
                document.DocumentVersions!.OrderByDescending(x => int.Parse(x.VersionNumber!)).FirstOrDefault()!
                    .DocumentVersionId,
            OrderIndex = orderMax + 1,
            DigitalCertificateId = cer.DigitalCertificateId
        };
        await _unitOfWork.DocumentSignatureUOW.AddAsync(docSign);
        await _unitOfWork.SaveChangesAsync();
        return ResponseUtil.GetObject(docSign, "Signature created successfully", HttpStatusCode.OK, 1);
    }
}