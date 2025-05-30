using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Option;
using DataAccess;
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
using MongoDB.Driver;
using Repository;
using Service.Response;
using Service.Utilities;
using Syncfusion.Pdf.Parsing;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfReader = iText.Kernel.Pdf.PdfReader;
using Scope = BusinessObject.Enums.Scope;

namespace Service.Impl;

public partial class ArchiveDocumentService : IArchiveDocumentService
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage");
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _host;
    private readonly IFileService _fileService;
    private readonly IDocumentService _documentService;
    private readonly MongoDbService _mongoDbService;
    private readonly ILoggingService _loggingService;


    public ArchiveDocumentService(IMapper mapper, IUnitOfWork unitOfWork,IOptions<AppsetingOptions> options, IFileService fileService, IDocumentService documentService, MongoDbService mongoDbService, ILoggingService loggingService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _documentService = documentService;
        _mongoDbService = mongoDbService;
        _loggingService = loggingService;
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
            SystemNumberDocument = x.SystemNumberOfDoc,
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
    
        if (!string.IsNullOrEmpty(request.SystemNumber))
                    data = data.FindAll(x => x.SystemNumberDocument.Contains(request.SystemNumber));
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
            request.StartCreatedDate = request.StartCreatedDate.Value.AddHours(-7);
        if (request.EndCreatedDate != null)
            request.EndCreatedDate = request.EndCreatedDate.Value.AddHours(16).AddMinutes(59).AddSeconds(59);
        data = ApplyFilters(data, request);
    
        data = request.SortByCreatedDate == SortByCreatedDate.Ascending
            ? data.OrderBy(x => x.CreatedDate).ToList()
            : data.OrderByDescending(x => x.CreatedDate).ToList();
    
        var paginatedData = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalPage = data.Count / pageSize + (data.Count % pageSize > 0 ? 1 : 0);
        return ResponseUtil.GetCollection(paginatedData, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, data.Count, page, pageSize, totalPage);
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
        var response = templates.Where(p => p.ArchivedDocumentStatus==ArchivedDocumentStatus.Archived && p.Page != -99).Select(x =>
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
        var totalPage = response.Count / pageSize + (response.Count % pageSize > 0 ? 1 : 0);
        return ResponseUtil.GetCollection(final, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, response.Count, page,
            pageSize, totalPage);
    }

    public async Task<ResponseDto> GetArchiveDocumentDetail(Guid documentId, Guid userId)
    {
        var docA = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(documentId);
        if (docA == null)
        {
            return ResponseUtil.Error("Document not found", ResponseMessages.FailedToSaveData, HttpStatusCode.NotFound);
        }

        var permissions = docA.UserDocumentPermissions;
        var userPermissions = permissions.Select(x => x.User).ToList();
        var ViewerList = new List<Viewer>();
        var GranterList = new List<Viewer>();
        ViewerList.AddRange(permissions.Select(x => x.User).Select(x =>
            {
                var viewer = new Viewer()
                {
                    FullName = x.FullName,
                    UserName = x.UserName,
                    Avatar = x.Avatar,
                    UserId = x.UserId,
                };
                return viewer;
            }
        ).ToList());
        GranterList.AddRange(permissions.Where(x => x.GrantPermission == GrantPermission.Grant).Select(x => x.User).Select(x =>
            {
                var viewer = new Viewer()
                {
                    FullName = x.FullName,
                    UserName = x.UserName,
                    Avatar = x.Avatar,
                    UserId = x.UserId,
                };
                return viewer;
            }
        ).ToList());
        var canGrant = permissions.Any(x => x.UserId == userId && x is { GrantPermission: GrantPermission.Grant, IsDeleted: false });
        var canDownLoad = permissions.Any(x => x.UserId == userId && x is { GrantPermission: GrantPermission.Download, IsDeleted: false });
        bool? canRevoke = null;
        
        if (docA.ArchivedDocumentStatus is not  (ArchivedDocumentStatus.Sent or ArchivedDocumentStatus.Withdrawn ) || docA is { DocumentReplaceId: not null, DocumentRevokeId: null })
        {
            canRevoke = null;
        }
        else if (docA is { ArchivedDocumentStatus: ArchivedDocumentStatus.Sent, DocumentRevokeId: null } )
        {
            if (docA.FinalDocument.Tasks.Any(x =>
                    x.UserId == userId && x.User.UserRoles.FirstOrDefault(x => x.IsPrimary).Role.RoleName != "Chief"))
                canRevoke = null;
            else
                canRevoke = true;
        }
        else if (docA is { ArchivedDocumentStatus: ArchivedDocumentStatus.Withdrawn, DocumentRevokeId: not null, DocumentReplaceId: null })
        {
            if (docA.FinalDocument.UserId != userId)
            {
                canRevoke = null;
            }
            else
                canRevoke = false;

        }
        // var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        // var dateNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var dateNow = DateTime.UtcNow;
        var isExpire = dateNow > docA.ExpirationDate;
        var createDate = dateNow;
        var issueDate = dateNow;
        var validFrom = dateNow;
        
        if (docA.Scope == Scope.InComing)
        {
            createDate = docA.CreatedDate;
            issueDate = docA.DateIssued ?? docA.CreatedDate;
            try
            {
                string fileExtension = ".conf-dms"; // Phần mở rộng tùy chỉnh
                string filePath = Path.Combine(_storagePath,"archive_document",docA.ArchivedDocumentId.ToString(), $"config{fileExtension}");

                // Kiểm tra xem tệp có tồn tại không
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Không tìm thấy tệp cấu hình.", filePath);
                }

                // Đọc nội dung từ tệp
                string content = await File.ReadAllTextAsync(filePath);

                // Chuyển chuỗi thành DateTime
                if (DateTime.TryParseExact(content, "yyyy-MM-dd HH:mm:ss", 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, 
                        out DateTime dateTime))
                {
                    validFrom = dateTime;
                }
                else
                {
                    throw new FormatException("Nội dung tệp không đúng định dạng DateTime (yyyy-MM-dd HH:mm:ss).");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đọc tệp.", ex);
            }
        }
        else
        {
            createDate = docA.CreatedDate;
            issueDate = docA.CreatedDate;
            validFrom = docA.DateIssued ?? docA.CreatedDate;
        }
        var sender = string.Empty;
        var receiver = string.Empty;
        if (docA.Scope == Scope.InComing)
        {
            sender = docA.Sender;
            receiver = docA.FinalDocument.User.UserName;
        }
        else
        {
            sender = docA.Sender;
            receiver = docA.ExternalPartner;
        }
        
        var AttachmentList = await _unitOfWork.AttachmentArchivedUOW.GetAttachmentArchivedDocumentByDocumentId(docA.ArchivedDocumentId);
        var attachments = AttachmentList.Select(x => new AttachmentArchivedDocumentDto()
        {
            AttachmentArchivedDocumentId = x.AttachmentArchivedDocumentId,
            AttachmentName = x.AttachmentName,
            AttachmentUrl = x.AttachmentUrl
        }).ToList();
        var result = new ArchiveDocumentResponse()
        {
            Granters = GranterList,
            DocumentId = docA.ArchivedDocumentId,
            DocumentName = docA.ArchivedDocumentName,
            DocumentContent = docA.ArchivedDocumentContent,
            NumberOfDocument = docA.NumberOfDocument,
            SystemNumberOfDocument = docA.SystemNumberOfDoc,
            Sender = sender,
            // ExternalPartner = docA.ExternalPartner,
            // ReceivedBy = receiver,
            CanGrant = canGrant,
            CanDownLoad = canDownLoad,
            ReceivedBy = receiver,
            CanRevoke = canRevoke,
            Viewers = ViewerList,
            DateExpires = docA.ExpirationDate,
            DateReceived = docA.DateReceived,
            CreateDate = createDate,
            ArchivedBy = docA.CreatedBy,
            ArchivedDate = docA.CreatedDate,
            Attachments = attachments,
            Scope = docA.Scope.ToString(),
            DocumentTypeName = docA.DocumentType?.DocumentTypeName,
            WorkflowName = string.Empty,
            IsExpire = isExpire,
            RevokeDocument = new SimpleDocumentResponse()
            {
                documentId = docA.DocumentRevokeId,
                DocumentName = docA.DocumentRevokes?.ArchivedDocumentName,
            },
            ReplacedDocument = new SimpleDocumentResponse()
            {
                documentId = docA.DocumentReplaceId,
                DocumentName = docA.DocumentReplaces?.ArchivedDocumentName,
            },
            Deadline = null,
            Status = docA.ArchivedDocumentStatus.ToString(),
            CreatedBy = docA.FinalDocument.User.UserName,
            DateIssued = issueDate,
            ValidFrom = validFrom,
            DigitalSignatures = docA.ArchiveDocumentSignatures?.Where(x => x.DigitalCertificate!=null).Where(x =>x.DigitalCertificate.IsUsb!=null).Select(x => new SignatureResponse()
            {
                SignerName = ExtractSigners(x.DigitalCertificate.Subject),
                SignedDate = x.SignedAt,
                IsDigital = true,
            }).ToList(),
            ApprovalSignatures = docA.ArchiveDocumentSignatures?.Where(x => x.DigitalCertificate!=null).Where(x =>x.DigitalCertificate.IsUsb==null).Select(x => new SignatureResponse()
            {
                SignerName = x.DigitalCertificate.User.FullName,
                SignedDate = x.SignedAt,
                IsDigital = false,
                ImgUrl = x.DigitalCertificate.SignatureImageUrl,
                
            }).ToList()
            ,
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

        var filter = Builders<Count>.Filter.Eq(x => x.Id, "base");
        var count = _mongoDbService.Counts.Find(filter).FirstOrDefault();
        var documentType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByIdAsync(archiveDocumentRequest.DocumentTypeId);
        count.Value += 1;
        var update = Builders<Count>.Update.Set(x => x.Value, count.Value);
        await _mongoDbService.Counts.UpdateOneAsync(filter, update);
        var template = new ArchivedDocument()
        {
            
            ArchivedDocumentId = templateId,
            SystemNumberOfDoc = (count.Value < 10 ? "0" + count.Value : count.Value) + "/" + count.UpdateTime.Year +
                                "/" + documentType.Acronym + "-TNABC",
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
            packageProperties.Subject = templateId.ToString();
        }
        extension = Path.GetExtension(filePath);
        template.ArchivedDocumentUrl = _host + "/api/ArchiveDocument/view-download-template?templateId=" + templateId + extension;

        await _unitOfWork.ArchivedDocumentUOW.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();
        await _loggingService.WriteLogAsync(userId, $"Tạo mẫu tài liệu {template.ArchivedDocumentName} thành công.");
        return ResponseUtil.GetObject("hehe", ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<IActionResult> DownloadTemplate(string templateId, Guid userId,bool? isPdf)
    {

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage","template", $"{templateId}");
        if (isPdf.HasValue && isPdf.Value)
        {
            await _loggingService.WriteLogAsync(userId, $"Tải mẫu tài liệu {templateId} thành công.");
            return await _fileService.ConvertDocToPdf(filePath);
        }
        await _loggingService.WriteLogAsync(userId, $"Tải mẫu tài liệu {templateId} thành công.");
        return await _fileService.GetPdfFile(filePath);
    }

    public async Task<ResponseDto> WithdrawArchiveDocument(Guid archiveDocumentId,DocumentPreInfo documentPreInfo, Guid userId)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(archiveDocumentId);
        if (archiveDoc == null)
        {
            return ResponseUtil.Error("Archive document not found",ResponseMessages.FailedToSaveData,HttpStatusCode.NotFound);
        }
        var newArchiveDocId = Guid.NewGuid();
        var newDocId =(Guid) (await  _documentService.CreateDocumentByTemplate(documentPreInfo, userId)).Content;
        var newDoc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(newDocId);
        
        var newArchiveDoc = new ArchivedDocument()
        {
            ArchivedDocumentId = newArchiveDocId,
            CreatedDate = DateTime.Now,
            SystemNumberOfDoc = newDoc.SystemNumberOfDoc,
            ArchivedDocumentStatus = ArchivedDocumentStatus.Archived,
            IsTemplate = false,
            DocumentTypeId = documentPreInfo.DocumentTypeId,
            DocumentRevokeId = archiveDoc.ArchivedDocumentId,
            Scope = archiveDoc.Scope,
            FinalDocumentId = newDocId
        };
        if (newDoc == null)
        {
            return ResponseUtil.Error("Create new document false",ResponseMessages.FailedToSaveData,HttpStatusCode.NotFound);
        }
        newDoc.FinalArchiveDocumentId = newArchiveDocId;
        archiveDoc.DocumentRevokeId = newArchiveDocId;
        await _unitOfWork.DocumentUOW.UpdateAsync(newDoc);
        await _unitOfWork.ArchivedDocumentUOW.AddAsync(newArchiveDoc);
        await _unitOfWork.SaveChangesAsync();
        await _loggingService.WriteLogAsync(userId,$"Thu hồi tài liệu {archiveDoc.SystemNumberOfDoc} thành công.");
        return ResponseUtil.GetObject(newDocId, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> ReplaceArchiveDocument(Guid archiveDocumentId, DocumentPreInfo documentPreInfo, Guid userId)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(archiveDocumentId);
        if (archiveDoc == null)
        {
            return ResponseUtil.Error("Archive document not found",ResponseMessages.FailedToSaveData,HttpStatusCode.NotFound);
        }
        var newArchiveDocId = Guid.NewGuid();
        var newDocId =(Guid) (await  _documentService.CreateDocumentByTemplate(documentPreInfo, userId)).Content;
        var newDoc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(newDocId);
        
        var newArchiveDoc = new ArchivedDocument()
        {
            ArchivedDocumentId = newArchiveDocId,
            CreatedDate = DateTime.Now,
            SystemNumberOfDoc = newDoc.SystemNumberOfDoc,
            ArchivedDocumentStatus = ArchivedDocumentStatus.Archived,
            IsTemplate = false,
            DocumentTypeId = documentPreInfo.DocumentTypeId,
            DocumentReplaceId = archiveDoc.ArchivedDocumentId,
            Scope = archiveDoc.Scope,
            FinalDocumentId = newDocId
        };
        if (newDoc == null)
        {
            return ResponseUtil.Error("Create new document false",ResponseMessages.FailedToSaveData,HttpStatusCode.NotFound);
        }
        newDoc.FinalArchiveDocumentId = newArchiveDocId;
        archiveDoc.DocumentReplaceId = newArchiveDocId;
        await _unitOfWork.DocumentUOW.UpdateAsync(newDoc);
        await _unitOfWork.ArchivedDocumentUOW.AddAsync(newArchiveDoc);
        await _unitOfWork.SaveChangesAsync();
        await _loggingService.WriteLogAsync(userId,$"Tạo văn bản thay thế {archiveDoc.SystemNumberOfDoc} thành công.");
        return ResponseUtil.GetObject(newDocId, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }
    
    
    public async Task<ResponseDto> WithdrawArchiveDocument(Guid userId, Guid archiveDocumentId)
    {
        try
        {
            var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(archiveDocumentId);
            if (archiveDoc == null)
            {
                return ResponseUtil.Error("Archive document not found", ResponseMessages.FailedToSaveData, HttpStatusCode.NotFound);
            }
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(archiveDoc.FinalDocumentId);
            if (document == null)
            {
                return ResponseUtil.Error("Document not found", ResponseMessages.FailedToSaveData, HttpStatusCode.NotFound);
            }
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            if (user == null)
            {
                return ResponseUtil.Error("User not found", ResponseMessages.FailedToSaveData, HttpStatusCode.NotFound);
            }
            var userDocPermission = await _unitOfWork.UserDocPermissionUOW.FindByUserIdAndArchiveDocAsync(userId, archiveDoc.ArchivedDocumentId);
            
            if (userDocPermission == null || userDocPermission.GrantPermission != GrantPermission.Grant)
            {
                return ResponseUtil.Error("You do not have permission to withdraw this document", 
                    ResponseMessages.OperationFailed, HttpStatusCode.Forbidden);
            }
            
            var scope = document.DocumentWorkflowStatuses.FirstOrDefault()?.Workflow.Scope;
            if (scope != Scope.OutGoing)
            {
                archiveDoc.ArchivedDocumentStatus = ArchivedDocumentStatus.Withdrawn;
                await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(archiveDoc);
                await _unitOfWork.SaveChangesAsync();
                await _loggingService.WriteLogAsync(user.UserId, $"Thu hồi tài liệu {archiveDoc.SystemNumberOfDoc} thành công bởi {user.FullName}.");
                return ResponseUtil.GetObject("Withdraw success", ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
            }
            else
            {
                return ResponseUtil.Error("Cannot use function for withdraw archive document in OutGoing scope", 
                    ResponseMessages.OperationFailed, HttpStatusCode.BadRequest);
            }
            
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed,
                HttpStatusCode.BadRequest);
        }
    }

    public async Task<ResponseDto> DeleteArchiveTemplate(Guid templateId, Guid userId)
    {
        var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(templateId);
        archiveDoc.Page = -99;
        await _unitOfWork.ArchivedDocumentUOW.UpdateAsync(archiveDoc);
        await _unitOfWork.SaveChangesAsync();
        await _loggingService.WriteLogAsync(userId, $"Xóa mẫu tài liệu {archiveDoc.ArchivedDocumentName} thành công.");
        return ResponseUtil.GetObject("Delete success", ResponseMessages.DeleteSuccessfully, HttpStatusCode.OK, 1);
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
