using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Option;
using DataAccess.DTO;
using DataAccess.DTO.Response;
using iText.Kernel.Pdf;
using iText.Signatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repository;
using Service.Utilities;

namespace Service.Impl;

public partial class DocumentService : IDocumentService
{
    private readonly IDigitalCertificateService _digitalCertificateService;
    private readonly IDocumentSignatureService _documentSignatureService;
    private readonly IExternalApiService _externalApiService;
    private readonly IFileService _fileService;
    private readonly string _host;
    private readonly ILogger<DocumentService> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;


    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService,
        ILogger<DocumentService> logger, IExternalApiService externalApiService,
        IDigitalCertificateService digitalCertificateService, IDocumentSignatureService documentSignatureService,
        IOptions<AppsetingOptions> options)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
        _logger = logger;
        _externalApiService = externalApiService;
        _digitalCertificateService = digitalCertificateService;
        _documentSignatureService = documentSignatureService;
        _host = options.Value.Host;
    }


    public async Task<ResponseDto> CreateIncomingDoc(DocumentUploadDto documentUploadDto, Guid userId)
    {
        string? GetString(object? obj)
        {
            return obj is JsonElement e ? e.GetString() : obj as string;
        }

        DateTime? GetDateTime(object? obj)
        {
            return obj is JsonElement e && DateTime.TryParse(e.GetString(), out var dt) ? dt : null;
        }

        Guid? GetGuid(object? obj)
        {
            return obj is JsonElement e && Guid.TryParse(e.GetString(), out var guid) ? guid : null;
        }

        var name = GetString(documentUploadDto.CanChange.GetValueOrDefault("Name"));
        var sender = GetString(documentUploadDto.CanChange.GetValueOrDefault("Sender"));
        var numberOfDocument = GetString(documentUploadDto.CanChange.GetValueOrDefault("NumberOfDocument"));
        var documentContent = GetString(documentUploadDto.CanChange.GetValueOrDefault("DocumentContent"));
        var dateReceived = GetDateTime(documentUploadDto.CanChange.GetValueOrDefault("DateReceived"));
        var documentTypeId = GetGuid(documentUploadDto.CanChange.GetValueOrDefault("DocumentTypeId"));
        var workflowId = GetGuid(documentUploadDto.CanChange.GetValueOrDefault("WorkflowId"));
        var deadline = GetDateTime(documentUploadDto.CanChange.GetValueOrDefault("Deadline")) ?? DateTime.Now;

        var workflowO = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
        var workflowFlow = workflowO.WorkflowFlows.Select(x => x).First(x => x.FlowNumber == 1);

        var document = new Document
        {
            DocumentName = name,
            DocumentContent = documentContent,
            NumberOfDocument = numberOfDocument,
            CreatedDate = DateTime.Now,
            DocumentTypeId = documentTypeId,
            ProcessingStatus = ProcessingStatus.InProgress,
            IsDeleted = false,
            Sender = sender,
            DateReceived = dateReceived,
            Deadline = deadline,
            UserId = userId,
            DocumentVersions = [],
            DocumentWorkflowStatuses =
            [
                new DocumentWorkflowStatus
                {
                    WorkflowId = (Guid)workflowId!,
                    StatusDoc = StatusDoc.Pending,
                    StatusDocWorkflow = StatusDocWorkflow.Pending,
                    CurrentWorkflowFlow = workflowFlow,
                    UpdatedAt = DateTime.Now
                }
            ]
        };

        var version = new DocumentVersion
        {
            VersionNumber = "1",
            CreateDate = DateTime.Now,
            IsFinalVersion = true
        };
        document.DocumentVersions.Add(version);
        await _unitOfWork.DocumentUOW.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        var url = _fileService.CreateAVersionFromUpload(documentUploadDto.CannotChange["fileName"]?.ToString(),
            version.DocumentVersionId, document.DocumentId, name);
        version.DocumentVersionUrl = url;

        document.DateIssued = DateTime.Today.ToString("yyyy-MM-dd");
        await _unitOfWork.DocumentUOW.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        if (workflowId == Guid.Empty)
            return ResponseUtil.Error("Invalid Workflow ID", "Operation Failed", HttpStatusCode.BadRequest);

        var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
        if (workflow == null)
            return ResponseUtil.Error("Workflow Not Found", "Operation Failed", HttpStatusCode.NotFound);

        var result = _mapper.Map<WorkflowDto>(workflow);
        var doc = _mapper.Map<DocumentDto>(document);
        doc.DocumentVersion = _mapper.Map<DocumentVersionDto>(version);

        return ResponseUtil.GetCollection(new List<object> { result, doc }, "Success", HttpStatusCode.OK, 2, 1, 2, 2);
    }

    public async Task<IActionResult> GetDocumentById(Guid documentId, string version)
    {
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        var versionId = document.DocumentVersions.FirstOrDefault(x => x.VersionNumber == version)?.DocumentVersionId;
        return await _fileService.GetPdfFile(Path.Combine("document", documentId.ToString(), versionId.ToString(),
            document.DocumentName + ".pdf"));
    }

    public async Task<ResponseDto> UpdateConfirmTaskWithDocument(Guid documentId)
    {
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        var version = document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion);
        switch (document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope)
        {
            case Scope.InComing:
                var archiveId = Guid.NewGuid();
                //TODO Send notification
                var archiveDocument = new ArchivedDocument
                {
                    ArchivedDocumentId = archiveId,
                    ArchivedDocumentName = document.DocumentName,
                    ArchivedDocumentContent = document.DocumentContent,
                    NumberOfDocument = document.NumberOfDocument,
                    CreatedDate = DateTime.Now,
                    Sender = document.Sender,
                    CreatedBy = document.User.UserName,
                    ExternalPartner = document.Sender,
                    ArchivedDocumentStatus = ArchivedDocumentStatus.Completed,
                    DateIssued = DateTime.Parse(document.DateIssued),
                    DateReceived = document.DateReceived,
                    DateSented = document.DateReceived,
                    DocumentRevokeId = null,
                    DocumentReplaceId = null,
                    Scope = document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope,
                    IsTemplate = false,
                    DocumentType = document.DocumentType,
                    FinalDocumentId = document.DocumentId,
                    ArchivedDocumentUrl = _fileService.ArchiveDocument(document.DocumentName, documentId,
                        version.DocumentVersionId, archiveId)
                };
                var listSignature = version.DocumentSignatures.Select(signature => new ArchiveDocumentSignature
                {
                    SignedAt = signature.SignedAt, OrderIndex = signature.OrderIndex,
                    DigitalCertificate = signature.DigitalCertificate
                }).ToList();
                archiveDocument.ArchiveDocumentSignatures = listSignature;
                document.ProcessingStatus = ProcessingStatus.Archived;
                await _unitOfWork.DocumentUOW.UpdateAsync(document);
                await _unitOfWork.ArchivedDocumentUOW.AddAsync(archiveDocument);
                await _unitOfWork.SaveChangesAsync();

                break;
            case Scope.OutGoing:
                break;
            case Scope.Division:
                break;
            case Scope.School:
                break;
            default:
                return ResponseUtil.Error("Invalid Workflow Scope", "Operation Failed", HttpStatusCode.BadRequest);
        }

        return ResponseUtil.GetObject("oke", "Success", HttpStatusCode.OK, 1);
    }

    public async Task<IActionResult> GetArchiveDocumentById(Guid documentId, string? version)
    {
        var document = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(documentId);
        return await _fileService.GetPdfFile(Path.Combine("archive_document", documentId.ToString(),
            document.ArchivedDocumentName + ".pdf"));
    }

    public async Task<ResponseDto> UploadDocument(IFormFile file, string userId)
    {
        var url = await _fileService.SaveUploadFile(file);
        var fileName = Path.GetFileName(url);
        var metaData = CheckMetaDataFile(url);
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(Guid.Parse(userId));

        if (metaData?.Any(meta => !meta.IsValid) == true)
            return ResponseUtil.Error("null", "Signature is not valid", HttpStatusCode.BadRequest);

        var aiResponse = await _externalApiService.ScanPdfAsync(url);

        var canChange = new Dictionary<string, object?>
        {
            { "Name", aiResponse.DocumentName },
            { "NumberOfDocument", aiResponse.NumberOfDocument },
            { "DocumentContent", aiResponse.DocumentContent },
            { "Sender", null },
            { "Receiver", user?.UserName },
            { "DateReceived", null },
            { "DocumentTypeId", null },
            { "WorkflowId", null },
            { "Deadline", null },
            { "NewSignerName", null }
        };

        var cannotChange = new Dictionary<string, object?>
        {
            { "SignatureName", metaData?.Select(x => x.SignatureName).ToList() },
            { "Issuer", metaData?.Select(x => x.Issuer).ToList() },
            { "SignerName", metaData?.Select(x => x.SignerName).ToList() },
            { "SingingDate", metaData?.Select(x => x.SingingDate).ToList() },
            { "Reason", metaData?.Select(x => x.Reason).ToList() },
            { "Location", metaData?.Select(x => x.Location).ToList() },
            { "IsValid", metaData?.Select(x => x.IsValid).ToList() },
            { "SerialNumber", metaData?.Select(x => x.SerialNumber).ToList() },
            { "ValidFrom", metaData?.Select(x => x.ValidFrom).ToList() },
            { "ExpirationDate", metaData?.Select(x => x.ExpirationDate).ToList() },
            { "Algorithm", metaData?.Select(x => x.Algorithm).ToList() },
            { "Valid", metaData?.Select(x => x.ValidFrom).ToList() },
            { "fileName", fileName }
        };

        var docDto = new DocumentUploadDto
        {
            CanChange = canChange,
            CannotChange = cannotChange
        };

        return ResponseUtil.GetObject(docDto, "oke", HttpStatusCode.OK, 1);
    }

    public Task<IActionResult> GetDocumentByName(string documentName)
    {
        return _fileService.GetPdfFile(documentName);
    }

    private async Task SaveDocumentAsync(Document document)
    {
        await _unitOfWork.DocumentUOW.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task ProcessMetaDataAsync(List<MetaDataDocument> metaData, Document document, Guid userId)
    {
        foreach (var (meta, index) in metaData.Select((meta, index) => (meta, index)))
        {
            var certificate =
                (DigitalCertificate)(await _digitalCertificateService.CreateCertificate(meta, userId)).Content;
            await _documentSignatureService.CreateSignature(document, certificate, meta, userId, index);
        }
    }

    private static List<string> ExtractSigners(List<DocumentSignature> signatures)
    {
        var regex = MyRegex();
        return signatures
            .Select(signature => regex.Match(signature.DigitalCertificate?.Subject ?? string.Empty))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .ToList();
    }

    private List<MetaDataDocument>? CheckMetaDataFile(string url)
    {
        if (!File.Exists(url))
        {
            _logger.LogError("File not found at {url}", url);
            return null;
        }

        using var pdfReader = new PdfReader(url);
        using var pdfDocument = new PdfDocument(pdfReader);
        var signatureUtil = new SignatureUtil(pdfDocument);
        var signatureNames = signatureUtil.GetSignatureNames();

        return signatureNames.Count > 0
            ? signatureNames.Select(name =>
            {
                var signature = signatureUtil.GetSignature(name);
                var pkcs7 = signatureUtil.ReadSignatureData(name);
                return new MetaDataDocument
                {
                    Issuer = pkcs7.GetSigningCertificate().GetIssuerDN().ToString(),
                    SignatureName = name,
                    SignerName = pkcs7.GetSigningCertificate().GetSubjectDN().ToString(),
                    SingingDate = pkcs7.GetSignDate(),
                    Reason = signature.GetReason(),
                    Location = signature.GetLocation(),
                    IsValid = pkcs7.VerifySignatureIntegrityAndAuthenticity(),
                    SerialNumber = pkcs7.GetSigningCertificate().GetSerialNumber().ToString(),
                    ValidFrom = pkcs7.GetSigningCertificate().GetNotBefore().ToLocalTime(),
                    ExpirationDate = pkcs7.GetSigningCertificate().GetNotAfter().ToLocalTime(),
                    Algorithm = pkcs7.GetSignatureAlgorithmName()
                };
            }).ToList()
            : null;
    }

    [GeneratedRegex(@"CN=([^,]+)")]
    private static partial Regex MyRegex();
}