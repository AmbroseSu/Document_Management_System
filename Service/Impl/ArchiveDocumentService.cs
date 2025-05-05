using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Option;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly string _host;
    private readonly IFileService _fileService;

    public ArchiveDocumentService(IMapper mapper, IUnitOfWork unitOfWork,IOptions<AppsetingOptions> options, IFileService fileService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _host = options.Value.Host;

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

    /// <summary>
    /// Retrieves all archived documents for a specific user, applies filters, and paginates the results.
    /// </summary>
    /// <param name="getAllArchiveRequestDto">The request DTO containing filter criteria.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page for pagination.</param>
    /// <returns>A paginated and filtered list of archived documents wrapped in a ResponseDto.</returns>
    public async Task<ResponseDto> GetAllArchiveDocuments(GetAllArchiveRequestDto getAllArchiveRequestDto, Guid userId, int page, int pageSize)
    {
        var cacheKey = "ArchiveDocumentUserId" + userId;
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<ArchiveResponseDto>>(cacheKey);
    
        if (cache != null) return FilterAndPaginateResponse(cache, getAllArchiveRequestDto, page, pageSize);
        var aDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByUserIdAsync(userId);
        var response = aDoc.Select(x => new ArchiveResponseDto
        {
            Id = x.ArchivedDocumentId,
            Name = x.ArchivedDocumentName,
            CreateDate = x.CreatedDate,
            Status = x.ArchivedDocumentStatus.ToString(),
            Type = x.DocumentType?.DocumentTypeName ?? string.Empty,
            SignBy = ExtractSigners(x.ArchiveDocumentSignatures?.Select(c => c.DigitalCertificate).FirstOrDefault()?.Subject ?? string.Empty),
            CreateBy = x.CreatedBy,
            NumberOfDocument = x.NumberOfDocument,
            CreatedDate = x.CreatedDate,
            Scope = x.Scope.ToString(),
            Sender = x.Sender,
            ExternalPartner = x.ExternalPartner,
            DateReceived = x.DateReceived,
            DateSented = x.DateSented
        }).ToList();
    
        await _unitOfWork.RedisCacheUOW.SetDataAsync(cacheKey, response, TimeSpan.FromMinutes(1));
        return FilterAndPaginateResponse(response, getAllArchiveRequestDto, page, pageSize);
    }
    
    /// <summary>
    /// Applies filters to a list of archived documents based on the provided request criteria.
    /// </summary>
    /// <param name="data">The list of archived documents to filter.</param>
    /// <param name="request">The request DTO containing filter criteria.</param>
    /// <returns>A filtered list of archived documents.</returns>
    private static List<ArchiveResponseDto> ApplyFilters(List<ArchiveResponseDto> data, GetAllArchiveRequestDto request)
    {
        if (!string.IsNullOrEmpty(request.Name))
            data = data.FindAll(x => x.Name.Contains(request.Name));
    
        if (request.Scope != null)
            data = data.FindAll(x => x.Scope == request.Scope.ToString());
    
        if (request.StartCreatedDate != null)
            data = data.FindAll(x => x.CreatedDate.CompareTo(request.StartCreatedDate) >= 0);
    
        if (request.EndCreatedDate != null)
            data = data.FindAll(x => x.CreatedDate.CompareTo(request.EndCreatedDate) <= 0);
    
        if (request.Status != null)
            data = data.FindAll(x => x.Status == request.Status.ToString());
    
        return data;
    }
    
    /// <summary>
    /// Filters and paginates a list of archived documents based on the provided request criteria.
    /// </summary>
    /// <param name="data">The list of archived documents to filter and paginate.</param>
    /// <param name="request">The request DTO containing filter and sorting criteria.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page for pagination.</param>
    /// <returns>A paginated and filtered ResponseDto containing the archived documents.</returns>
    private static ResponseDto FilterAndPaginateResponse(List<ArchiveResponseDto> data, GetAllArchiveRequestDto request, int page, int pageSize)
    {
        if (request.StartCreatedDate != null)
            request.StartCreatedDate = request.StartCreatedDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
        if (request.EndCreatedDate != null)
            request.EndCreatedDate = request.EndCreatedDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
        data = ApplyFilters(data, request);
    
        data = request.SortByCreatedDate == SortByCreatedDate.Ascending
            ? data.OrderBy(x => x.CreatedDate).ToList()
            : data.OrderByDescending(x => x.CreatedDate).ToList();
    
        var paginatedData = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return ResponseUtil.GetCollection(paginatedData, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, data.Count, page, pageSize, data.Count);
    }

    public async Task<ResponseDto> GetAllArchiveTemplates(string? documentType,string? name,int page, int pageSize)
    {
        var templates = (await _unitOfWork.ArchivedDocumentUOW.GetAllArchiveTemplates()).AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
        {
            templates = templates.Where(x => x.ArchivedDocumentName.ToLower().Contains(name.ToLower()));
        }
        if (!string.IsNullOrWhiteSpace(documentType))
        {
            templates = templates.Where(x => x.DocumentType.DocumentTypeName.ToLower().Contains(documentType.ToLower()));
        }
        var response = templates.Where(p => p.ArchivedDocumentStatus==ArchivedDocumentStatus.Archived).Select(x =>
            new
            {
                Id = x.ArchivedDocumentId,
                Name = x.ArchivedDocumentName,
                CreateBy = x.CreatedBy,
                CreateDate = x.CreatedDate,
                Type = x.DocumentType.DocumentTypeName ?? string.Empty,
                Url = x.ArchivedDocumentUrl
                
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
        var originalPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage","template");
        if (!Directory.Exists(originalPath))
        {
            Directory.CreateDirectory(originalPath);
        }
        var filePath = Path.Combine(originalPath, $"{templateId}.{archiveDocumentRequest.Template.FileName.Split('.').Last()}");
        var extension = Path.GetExtension(archiveDocumentRequest.Template.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await archiveDocumentRequest.Template.CopyToAsync(stream);
        }

        if (extension == ".doc")
        {
            _fileService.ConvertDocToDocx(filePath, originalPath);
            File.Delete(filePath);
            filePath = Path.Combine(originalPath, $"{templateId}.docx");
        }


        // Add metadata to the .docx file
        using (var wordDoc = WordprocessingDocument.Open(filePath, true))
        {
            var packageProperties = wordDoc.PackageProperties;
            packageProperties.Identifier = templateId.ToString();
        }
        extension = Path.GetExtension(filePath);
        template.ArchivedDocumentUrl = _host + "/api/ArchiveDocument/view-download-template?templateId=" + templateId + extension;

        await _unitOfWork.ArchivedDocumentUOW.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();
        return ResponseUtil.GetObject("hehe", ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    public Task<IActionResult> DownloadTemplate(string templateId, Guid userId,bool? isPdf)
    {

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage","template", $"{templateId}");
        if (isPdf.HasValue && isPdf.Value)
            return _fileService.ConvertDocToPdf(filePath);
        return _fileService.GetPdfFile(filePath);
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
