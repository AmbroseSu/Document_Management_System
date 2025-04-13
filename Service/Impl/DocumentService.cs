using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using DataAccess.DTO;
using DataAccess.DTO.Response;
using iText.Kernel.Pdf;
using iText.Signatures;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repository;
using Service.Utilities;

namespace Service.Impl;

public partial class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;
    private readonly ILogger<DocumentService> _logger;
    private readonly IExternalApiService _externalApiService;
    private readonly IDigitalCertificateService _digitalCertificateService;
    private readonly IDocumentSignatureService _documentSignatureService;

    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService,
        ILogger<DocumentService> logger, IExternalApiService externalApiService,
        IDigitalCertificateService digitalCertificateService, IDocumentSignatureService documentSignatureService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
        _logger = logger;
        _externalApiService = externalApiService;
        _digitalCertificateService = digitalCertificateService;
        _documentSignatureService = documentSignatureService;
    }
    
    public async Task<ResponseDto> InsertSimpleDocument(DocumentUploadDto documentUploadDto)
    {
        
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentUploadDto.DocumentId);
        
        if (document == null)
        {
            return ResponseUtil.Error("null", "Document not found", HttpStatusCode.NotFound);
        }
        
        document.DocumentName = documentUploadDto.Name;
        document.Sender = documentUploadDto.Sender;
        document.DateReceived = documentUploadDto.DateReceived;
        document.NumberOfDocument = documentUploadDto.NumberOfDocument;
        document.DocumentType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByNameAsync(documentUploadDto.DocumentTypeName);
        if (documentUploadDto.SignBys.Count > 1)
        {
            document.DocumentVersions!.OrderByDescending(v => int.Parse(v.VersionNumber))
                .FirstOrDefault()
                .DocumentSignatures = documentUploadDto.SignBys.Select((v,index) => new DocumentSignature
                {
                    SignedAt = v.SignAt,
                    OrderIndex = index+1,
                    DigitalCertificate = new DigitalCertificate()
                    {
                        Subject = v.Name
                    }
                }).ToList();
                
        }
        
        var workflow =await _unitOfWork.WorkflowUOW.FindWorkflowByNameAsync(documentUploadDto.WorkflowName);
        
        var documentWorkflowStatus = new DocumentWorkflowStatus()
        {
            StatusDocWorkflow = StatusDocWorkflow.Pending,
            StatusDoc = StatusDoc.Pending,
            UpdatedAt = DateTime.Now,
            DocumentId = documentUploadDto.DocumentId,
            WorkflowId = workflow!.WorkflowId,
            CurrentWorkflowFlowId = workflow.WorkflowFlows!.FirstOrDefault()!.WorkflowId
        };
        await _unitOfWork.DocumentWorkflowStatusUOW.AddAsync(documentWorkflowStatus);
        await _unitOfWork.SaveChangesAsync();
        return ResponseUtil.GetObject(null!, "oke", HttpStatusCode.OK, 0);
    }
    
    public async Task<ResponseDto> UploadDocument(IFormFile file, string? userId)
    {
        var url = await _fileService.SaveFile(file);
        var metaData = CheckMetaDataFile(url);

        if (metaData?.Any(meta => !meta.ValidTo) == true)
            return ResponseUtil.Error("null", "Signature is not valid", HttpStatusCode.BadRequest);

        var aiResponse = await _externalApiService.ScanPdfAsync(url);
        var docType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByNameAsync(aiResponse.DocumentType);

        var listSignature = metaData?.Select((signature, index) => new DocumentSignature
        {
            SignedAt = signature.SingingDate,
            OrderIndex = index + 1,
            DigitalCertificate = new DigitalCertificate
            {
                Subject = signature.SignerName,
                SerialNumber = signature.SerialNumber,
                ValidFrom = signature.ValidFrom,
                Issuer = signature.Issuer,
                ValidTo = signature.ExpirationDate
            }
        }).ToList() ?? new List<DocumentSignature>();

        var document = new Document
        {
            ProcessingStatus = ProcessingStatus.InProgress,
            DocumentName = aiResponse.DocumentName,
            DocumentContent = aiResponse.DocumentContent,
            DateIssued = aiResponse.DateIssued,
            NumberOfDocument = aiResponse.NumberOfDocument,
            CreatedDate = DateTime.UtcNow,
            UserId = Guid.Parse(userId),
            IsDeleted = false,
            DocumentType = docType,
            DocumentVersions = new List<DocumentVersion>
            {
                new DocumentVersion
                {
                    VersionNumber = "1",
                    DocumentVersionUrl = url,
                    CreateDate = DateTime.Now,
                    IsFinalVersion = true,
                    DocumentSignatures = listSignature
                }
            }
        };

        await SaveDocumentAsync(document);

        if (metaData != null)
            await ProcessMetaDataAsync(metaData, document, Guid.Parse(userId));

        var signBy = ExtractSigners(document.DocumentVersions
            .OrderByDescending(v => int.Parse(v.VersionNumber!))
            .FirstOrDefault()?.DocumentSignatures ?? new List<DocumentSignature>());

        var docDto = new DocumentUploadDto
        {
            DocumentId = document.DocumentId,
            Name = document.DocumentName,
            Sender = document.Sender,
            DateReceived = document.DateReceived,
            NumberOfDocument = document.NumberOfDocument,
            DocumentTypeName = document.DocumentType?.DocumentTypeName,
            WorkflowName = document.DocumentWorkflowStatuses?.FirstOrDefault()?.Workflow?.WorkflowName,
            SignByName = signBy,
            DocumentContent = document.DocumentContent
        };

        return ResponseUtil.GetObject(docDto, "oke", HttpStatusCode.OK, 1);
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
            var certificate = (DigitalCertificate)(await _digitalCertificateService.CreateCertificate(meta, userId)).Content;
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
                    ValidTo = pkcs7.VerifySignatureIntegrityAndAuthenticity(),
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