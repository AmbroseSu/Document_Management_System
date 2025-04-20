using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using iText.Signatures;
//using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Repository;
using Service.Response;
using Service.Utilities;
using Syncfusion.Pdf.Parsing;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfReader = iText.Kernel.Pdf.PdfReader;

namespace Service.Impl;

public partial class ArchiveDocumentService : IArchiveDocumentService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveDocumentService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    /*public string ExtractSignatures(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        throw new ArgumentException("File không hợp lệ.");
    }

    using (var memoryStream = new MemoryStream())
    {
        // Đọc nội dung file vào MemoryStream
        file.OpenReadStream().CopyTo(memoryStream);

        // Đảm bảo rằng chúng ta có thể đọc lại từ MemoryStream
        memoryStream.Position = 0;

        try
        {
            // Mở tài liệu PDF bằng Syncfusion
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(memoryStream))
            {
                var signatureDetails = new List<string>();

                // Lặp qua tất cả các chữ ký trong tài liệu PDF
                foreach (var signature in loadedDocument.Signatures)
                {
                    if (signature is PdfSignature pdfSignature)
                    {
                        // Kiểm tra trạng thái chữ ký
                        if (pdfSignature.SignatureVerificationStatus == SignatureVerificationStatus.Signed)
                        {
                            // Thêm thông tin chữ ký vào danh sách
                            string signatureInfo = $"Signed By: {pdfSignature.SignerName}, " +
                                                   $"Signed Date: {pdfSignature.SigningDate}, " +
                                                   $"Valid: {pdfSignature.IsValid}";

                            // Lấy thông tin chứng chỉ
                            X509Certificate2 cert = pdfSignature.Certificate;
                            signatureInfo += $", Cert Subject: {cert.Subject}, " +
                                             $"Cert Issuer: {cert.Issuer}, " +
                                             $"Valid From: {cert.NotBefore}, " +
                                             $"Valid To: {cert.NotAfter}";

                            signatureDetails.Add(signatureInfo);
                        }
                    }
                }

                // Trả về thông tin chữ ký dưới dạng chuỗi
                return string.Join(Environment.NewLine, signatureDetails);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Không thể xử lý PDF: {ex.Message}");
        }
    }
}*/

    public async Task<ResponseDto> GetAllArchiveDocuments(Guid userId, int page, int pageSize)
    {
        var cache = _unitOfWork.RedisCacheUOW.GetData<List<object>>("ArchiveDocumentUserId" + userId);
        IEnumerable<ArchivedDocument> aDoc;
        if (cache == null)
        {
            aDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByUserIdAsync(userId);
        }
        else
        {
            return ResponseUtil.GetCollection(cache.Skip((page - 1) * pageSize).Take(pageSize).ToList(), ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 10, page,
                pageSize, cache.Count);
        }

        
        var response = aDoc.Select(x =>
            new
            {
                Id = x.ArchivedDocumentId,
                Name = x.ArchivedDocumentName,
                CreateDate = x.CreatedDate,
                Status = x.ArchivedDocumentStatus.ToString(),
                Type = x.DocumentType?.DocumentTypeName ?? string.Empty,
                SignBy = ExtractSigners(
                    x.ArchiveDocumentSignatures?
                        .Select(c => c.DigitalCertificate)
                        .FirstOrDefault()?.Subject ?? string.Empty
                )
            }).ToList();

        _unitOfWork.RedisCacheUOW.SetData("ArchiveDocumentUserId" + userId, response,TimeSpan.FromMinutes(1));
        response.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ResponseUtil.GetCollection(response, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, response.Count, page,
            pageSize, (long)Math.Ceiling((double)(response.Count/pageSize)));
    }

    public async Task<ResponseDto> GetAllArchiveTemplates(int page, int pageSize)
    {
        var templates = await _unitOfWork.ArchivedDocumentUOW.GetAllArchiveTemplates();
        throw new NotImplementedException();
    }

    private static string ExtractSigners(string? signature)
    {
        var regex = MyRegex();
        var result = regex.Match(signature ?? string.Empty);
        var extracted = result.Success ? result.Groups[1].Value : string.Empty;
        return extracted;
    }
    [GeneratedRegex(@"CN=([^,]+)")]
    private static partial Regex MyRegex();
}
