using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using DocumentFormat.OpenXml.Office.CustomXsn;
using DocumentFormat.OpenXml.Packaging;
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
                ),
                CreateBy = x.CreatedBy,
                Statuss = x.ArchivedDocumentStatus.ToString(),
                x.NumberOfDocument,
                x.CreatedDate,
                x.CreatedBy,
                Scope = x.Scope.ToString(),
                x.Sender,
                x.ExternalPartner,
                x.DateReceived,
                x.DateSented
            }).ToList();

        _unitOfWork.RedisCacheUOW.SetData("ArchiveDocumentUserId" + userId, response,TimeSpan.FromMinutes(1));
        var final = response.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var total = (int)Math.Ceiling((double)(response.Count / pageSize));
        return ResponseUtil.GetCollection(final, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, response.Count, page,
            pageSize, total);
    }

    public async Task<ResponseDto> GetAllArchiveTemplates(int page, int pageSize)
    {
        var templates = await _unitOfWork.ArchivedDocumentUOW.GetAllArchiveTemplates();
        var response = templates.Select(x =>
            new
            {
                Id = x.ArchivedDocumentId,
                Name = x.ArchivedDocumentName,
                CreateDate = x.CreatedDate,
                Type = x.DocumentType?.DocumentTypeName ?? string.Empty,
                
            }).ToList();
        var final = response.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var total = (int)Math.Ceiling((double)(response.Count / pageSize));
        return ResponseUtil.GetCollection(final, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, response.Count, page,
            pageSize, total);
    }

    public async Task<ResponseDto> GetArchiveDocumentDetail(Guid documentId, Guid userId)
    {
        var docA = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(documentId);
        var result = new DocumentResponse()
        {
            DocumentId = docA.ArchivedDocumentId,
            DocumentName = docA.ArchivedDocumentName,
            DocumentContent = docA.ArchivedDocumentContent,
            NumberOfDocument = docA.NumberOfDocument,
            Sender = docA.Sender,
            DateReceived = docA.DateReceived,
            DocumentTypeName = docA.DocumentType?.DocumentTypeName,
            WorkflowName = string.Empty,
            Deadline = DateTime.MaxValue,
            Status = docA.ArchivedDocumentStatus.ToString(),
            CreatedBy = docA.CreatedBy,
            DateIssued = docA.DateIssued,
            DateExpires = DateTime.MaxValue,
            Signatures = docA.ArchiveDocumentSignatures?.Select(x => new SignatureResponse()
            {
                SignerName = ExtractSigners(x.DigitalCertificate?.Subject),
                SignedDate = x.SignedAt,
                IsDigital = x.DigitalCertificate is { SerialNumber: not null },
            }).ToList(),
            Versions = [
                new VersionDetailRespone()
                {
                    VersionNumber = "0",
                    CreatedDate = docA.CreatedDate,
                    Url = docA.ArchivedDocumentUrl,
                    IsFinal = true
                }
            ],
            Tasks = []
        };
        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK,1);
    }

    public async Task<ResponseDto> CreateArchiveTemplate(ArchiveDocumentRequest archiveDocumentRequest, Guid userId)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        var templateId = Guid.NewGuid();
        var template = new ArchivedDocument()
        {
            ArchivedDocumentId = templateId,
            ArchivedDocumentName = archiveDocumentRequest.TemplateName,
            CreatedBy = user.UserName,
            CreatedDate = DateTime.Now,
            DocumentTypeId = archiveDocumentRequest.DocumentTypeId,
            ArchivedDocumentStatus = ArchivedDocumentStatus.Archived,
            IsTemplate = true,
            Llx = archiveDocumentRequest.Llx,
            Lly = archiveDocumentRequest.Lly,
            Urx = archiveDocumentRequest.Urx,
            Ury = archiveDocumentRequest.Ury,
            Page = archiveDocumentRequest.Page,
        };
        // Save the file to a specified path
        var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "data", "storage"), $"{templateId}.{archiveDocumentRequest.Template.FileName.Split('.').Last()}");
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await archiveDocumentRequest.Template.CopyToAsync(stream);
        }

        // Add metadata to the .docx file
        using (var wordDoc = WordprocessingDocument.Open(filePath, true))
        {
            var packageProperties = wordDoc.PackageProperties;
            packageProperties.Identifier = templateId.ToString();
        }
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
