using System.Net;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;
    private readonly ILogger<DocumentService> _logger;
    private readonly IExternalApiService _externalApiService;
    private readonly IDigitalCertificateService _digitalCertificateService;
    private readonly IDocumentSignatureService _documentSignatureService;
    private readonly string _host;


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
    
    public async Task<ResponseDto> CreateDoc(DocumentUploadDto documentUploadDto,Guid userId)
    {
        
        var name = documentUploadDto.CanChange.TryGetValue("Name", out var value) 
            ? (string?)value 
            : null;

        var sender = documentUploadDto.CanChange.TryGetValue("Sender", out var value1) 
            ? (string?)value1 
            : null;

        var numberOfDocument = documentUploadDto.CanChange.TryGetValue("NumberOfDocument", out var value2) 
            ? (string?)value2 
            : null;

        var documentContent = documentUploadDto.CanChange.TryGetValue("DocumentContent", out var value3) 
            ? (string?)value3 
            : null;

        var receiver = documentUploadDto.CanChange.TryGetValue("Receiver", out var value4) 
            ? (string?)value4 
            : null;

        var dateReceived = documentUploadDto.CanChange.TryGetValue("DateReceived", out var value5) 
            ? (DateTime?)value5 
            : null;

        var documentTypeId = documentUploadDto.CanChange.TryGetValue("DocumentTypeId", out var value6) 
            ? (Guid?)value6 
            : null;

        var workflowId = documentUploadDto.CanChange.TryGetValue("WorkflowId", out var value7) 
            ? (Guid?)value7 
            : null;

        var deadline = documentUploadDto.CanChange.TryGetValue("Deadline", out var value8) 
            ? (DateTime)value8 
            : DateTime.Now;

        var newSignerName = documentUploadDto.CanChange.TryGetValue("NewSignerName", out var value9) 
            ? (string?)value9 
            : null;
        
        
        var signatureNames = documentUploadDto.CannotChange.TryGetValue("SignatureName", out var value10) 
            ? value10 as List<string> 
            : null;

        var issuers = documentUploadDto.CannotChange.TryGetValue("Issuer", out var value11) 
            ? value11 as List<string> 
            : null;

        var signerNames = documentUploadDto.CannotChange.TryGetValue("SignerName", out var value12) 
            ? value12 as List<string> 
            : null;

        var singingDates = documentUploadDto.CannotChange.TryGetValue("SingingDate", out var value13) 
            ? value13 as List<DateTime> 
            : null;

        var reasons = documentUploadDto.CannotChange.TryGetValue("Reason", out var value14) 
            ? value14 as List<string> 
            : null;

        var locations = documentUploadDto.CannotChange.TryGetValue("Location", out var value15) 
            ? value15 as List<string> 
            : null;

        var isValid = documentUploadDto.CannotChange.TryGetValue("IsValid", out var value16) 
            ? value16 as List<bool> 
            : null;

        var serialNumbers = documentUploadDto.CannotChange.TryGetValue("SerialNumber", out var value17) 
            ? value17 as List<string> 
            : null;

        var validFroms = documentUploadDto.CannotChange.TryGetValue("ValidFrom", out var value18) 
            ? value18 as List<DateTime> 
            : null;

        var expirationDates = documentUploadDto.CannotChange.TryGetValue("ExpirationDate", out var value19) 
            ? value19 as List<DateTime> 
            : null;

        var algorithms = documentUploadDto.CannotChange.TryGetValue("Algorithm", out var value20) 
            ? value20 as List<string> 
            : null;

        var document = new Document()
        {
            //TODO missing DateIssued
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
            UserId = userId
        };
        var version = new DocumentVersion()
        {
            //TODO missing docUrl
            VersionNumber = "1",
            CreateDate = DateTime.Now,
            IsFinalVersion = true,
        };

        var index = 0;
        if (singingDates != null)
        {
            
            var signatures = singingDates.Select(si =>
                {
                    if (validFroms != null && expirationDates != null )
                            return new DocumentSignature()
                            {
                                SignedAt = si,
                                DigitalCertificate = new DigitalCertificate()
                                {
                                    Subject = signerNames?[index],
                                    SerialNumber = serialNumbers?[index],
                                    ValidTo = expirationDates[index],
                                    Issuer = issuers?[index],
                                    ValidFrom = validFroms[index]
                                },
                                OrderIndex = index++
                            };

                    return null;
                })
                .ToList();
            version.DocumentSignatures = signatures;
            if (document.DocumentVersions != null) document.DocumentVersions.Add(version);
            else
            {
                document.DocumentVersions =
                [
                    version
                ];
            }
            await _unitOfWork.DocumentUOW.AddAsync(document);
        }
        else
        {
            return ResponseUtil.Error("singingDates null", "singingDates null", HttpStatusCode.BadRequest);
        }
        

        return ResponseUtil.GetObject(document, "oke", HttpStatusCode.OK, 0);
    }
    
    public Task<IActionResult> GetDocumentById(Guid documentId)
    {
        return _fileService.GetPdfFile(documentId);
    }

    public Task<IActionResult> GetDocumentByName(string documentName)
    {
        return _fileService.GetPdfFile(documentName);
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
        
        var canChange = new Dictionary<string, object?>();
        var cannotChange = new Dictionary<string, object?>();
        
        canChange.Add("Name", aiResponse.DocumentName);
        canChange.Add("NumberOfDocument", aiResponse.NumberOfDocument);
        canChange.Add("DocumentContent",aiResponse.DocumentContent);
        canChange.Add("Sender",null);
        canChange.Add("Receiver",user?.UserName);
        canChange.Add("DateReceived", null);
        canChange.Add("DocumentTypeId",null);
        canChange.Add("WorkflowId",null);
        canChange.Add("Deadline",null);
        canChange.Add("NewSignerName",null);
        
        cannotChange.Add("SignatureName",metaData?.Select(x => x.SignatureName).ToList());
        cannotChange.Add("Issuer",metaData?.Select(x => x.Issuer).ToList());
        cannotChange.Add("SignerName",metaData?.Select(x => x.SignerName).ToList());
        cannotChange.Add("SingingDate",metaData?.Select(x => x.SingingDate).ToList());
        cannotChange.Add("Reason",metaData?.Select(x => x.Reason).ToList());
        cannotChange.Add("Location",metaData?.Select(x => x.Location).ToList());
        cannotChange.Add("IsValid",metaData?.Select(x => x.IsValid).ToList());
        cannotChange.Add("SerialNumber",metaData?.Select(x => x.SerialNumber).ToList());
        cannotChange.Add("ValidFrom",metaData?.Select(x => x.ValidFrom).ToList());
        cannotChange.Add("ExpirationDate",metaData?.Select(x => x.ExpirationDate).ToList());
        cannotChange.Add("Algorithm",metaData?.Select(x => x.Algorithm).ToList());
        cannotChange.Add("Valid",metaData?.Select(x => x.ValidFrom).ToList());
        cannotChange.Add("fileName",fileName);

        var docDto = new DocumentUploadDto
        {
            CanChange = canChange,
            CannotChange = cannotChange
        };
        
        // var listSignature = metaData?.Select((signature, index) => new DocumentSignature
        // {
        //     SignedAt = signature.SingingDate,
        //     OrderIndex = index + 1,
        //     DigitalCertificate = new DigitalCertificate
        //     {
        //         Subject = signature.SignerName,
        //         SerialNumber = signature.SerialNumber,
        //         ValidFrom = signature.ValidFrom,
        //         Issuer = signature.Issuer,
        //         ValidTo = signature.ExpirationDate
        //     }
        // }).ToList() ?? [];

        // var document = new Document
        // {
        //     ProcessingStatus = ProcessingStatus.InProgress,
        //     DocumentName = aiResponse.DocumentName,
        //     DocumentContent = aiResponse.DocumentContent,
        //     DateIssued = aiResponse.DateIssued,
        //     NumberOfDocument = aiResponse.NumberOfDocument,
        //     CreatedDate = DateTime.UtcNow,
        //     UserId = Guid.Parse(userId),
        //     IsDeleted = false,
        //     DocumentType = docType,
        //     DocumentVersions = new List<DocumentVersion>
        //     {
        //         new DocumentVersion
        //         {
        //             VersionNumber = "1",
        //             DocumentVersionUrl = url,
        //             CreateDate = DateTime.Now,
        //             IsFinalVersion = true,
        //             DocumentSignatures = listSignature
        //         }
        //     }
        // };
        //
        // await SaveDocumentAsync(document);

        // if (metaData != null)
        //     await ProcessMetaDataAsync(metaData, document, Guid.Parse(userId));

        // var signBy = ExtractSigners(document.DocumentVersions
        //     .OrderByDescending(v => int.Parse(v.VersionNumber!))
        //     .FirstOrDefault()?.DocumentSignatures ?? new List<DocumentSignature>());

        // var docDto = new DocumentUploadDto
        // {
        //     url = _host+"/api/Document/view-download-document-by-name?documentName="+fileName,
        //     DocumentId = document.DocumentId,
        //     Name = document.DocumentName,
        //     Sender = document.Sender,
        //     DateReceived = document.DateReceived,
        //     NumberOfDocument = document.NumberOfDocument,
        //     DocumentTypeName = document.DocumentType?.DocumentTypeName,
        //     WorkflowName = document.DocumentWorkflowStatuses?.FirstOrDefault()?.Workflow?.WorkflowName,
        //     SignByName = signBy,
        //     DocumentContent = document.DocumentContent
        // };

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