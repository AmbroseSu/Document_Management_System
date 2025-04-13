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
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<DocumentService> _logger;
    private readonly IExternalApiService _externalApiService;
    private readonly IDigitalCertificateService _digitalCertificateService;
    private readonly IDocumentSignatureService _documentSignatureService;

    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService,
        IWorkflowService workflowService, ILogger<DocumentService> logger, IExternalApiService externalApiService,
        IDigitalCertificateService digitalCertificateService, IDocumentSignatureService documentSignatureService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
        _workflowService = workflowService;
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

    /// <summary>
    /// Uploads a document, processes its metadata, and returns a response containing document details.
    /// </summary>
    /// <param name="file">The file to be uploaded.</param>
    /// <param name="userId">The ID of the user uploading the document.</param>
    /// <returns>A <see cref="ResponseDto"/> containing the document details and status.</returns>
    public async Task<ResponseDto> UploadDocument(IFormFile file, Guid userId)
    {
        // Save the uploaded file and get its URL
        var url = await _fileService.SaveFile(file);

        // Check for metadata in the uploaded file
        var metaData = CheckMetaDataFile(url);

        if (metaData != null && metaData.Any(meta => !meta.ValidTo))
        {
            return ResponseUtil.Error("null", "Signature is not valid", HttpStatusCode.BadRequest);
        }
        // Scan the PDF using an external API to extract AI-generated information
        var aiResponse = await _externalApiService.ScanPdfAsync(url);

        // Find the document type based on the AI response
        var docType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByNameAsync(aiResponse.DocumentType);
        var listSignature = new List<DocumentSignature>();
        if (metaData != null)
        {
             listSignature = metaData.Select((signature, index) => new DocumentSignature
            {
                SignedAt = signature.SingingDate,
                OrderIndex = index + 1, // Bắt đầu từ 1
                DigitalCertificate = new DigitalCertificate
                {
                    Subject = signature.SignerName,
                    SerialNumber = signature.SerialNumber,
                    ValidFrom = signature.ValidFrom,
                    Issuer = signature.Issuer,
                    ValidTo = signature.ExpirationDate
                    
                }
                
            }).ToList();
        }

        // Create a new document object with the extracted information
        var document = new Document
        {
            DocumentName = aiResponse.DocumentName,
            DocumentContent = aiResponse.DocumentContent,
            DateIssued = aiResponse.DateIssued,
            NumberOfDocument = aiResponse.NumberOfDocument,
            CreatedDate = DateTime.UtcNow,
            UserId = userId,
            IsDeleted = false,
            //DocumentSignatures = [], // Initialize an empty list for document signatures
            DocumentType = docType,
            DocumentVersions =
            [
                new DocumentVersion()
                {
                    VersionNumber = 1.ToString(),
                    DocumentVersionUrl = url,
                    CreateDate = DateTime.Now,
                    IsFinalVersion = true,
                    DocumentSignatures = listSignature,
                }
            ]
        };

        // Save the document to the database
        await SaveDocumentAsync(document);

        // If metadata is found, process it and associate it with the document
        if (metaData != null)
        {
            await ProcessMetaDataAsync(metaData, document, userId);
        }

        // Extract the names of the signers from the document signatures
        var signBy = ExtractSigners(document.DocumentVersions?
            .OrderByDescending(v => int.Parse(v.VersionNumber!))
            .FirstOrDefault()?
            .DocumentSignatures?.ToList() ?? []);

        // Create a response object with the document details
        var docDto = new DocumentUploadDto()
        {
            DocumentId = document.DocumentId,
            Name = document.DocumentName,
            Sender = document.Sender,
            DateReceived = document.DateReceived,
            //ValidFrom = document.DocumentSignatures.Max(signature => signature.SignedAt), // Get the latest signing date
            //ValidTo = document.DocumentSignatures.Min(signature => signature.DigitalCertificate?.ValidTo), // Get the earliest expiration date
            NumberOfDocument = document.NumberOfDocument,
            DocumentTypeName = document.DocumentType?.DocumentTypeName,
            WorkflowName = document.DocumentWorkflowStatuses?[0].Workflow?.WorkflowName, // Get the workflow name if available
            SignByName = signBy,
            DocumentContent = document.DocumentContent
        };

        // Return the response object with a success message and status code
        return ResponseUtil.GetObject(docDto, "oke", HttpStatusCode.OK, 1);
    }

    private async Task SaveDocumentAsync(Document document)
    {
        await _unitOfWork.DocumentUOW.AddAsync(document);
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception("Error saving document: " + e.Message);
        }
    }

    private async Task ProcessMetaDataAsync(List<MetaDataDocument> metaData, Document document, Guid userId)
    {
        for (var index = 0; index < metaData.Count; index++)
        {
            var meta = metaData[index];
            var certificate =
                (DigitalCertificate)(await _digitalCertificateService.CreateCertificate(meta, userId)).Content;
            var signature =
                (DocumentSignature)(await _documentSignatureService.CreateSignature(document, certificate, meta, userId,
                    index)).Content;

            //document.DocumentSignatures.Add(signature);
            await SaveDocumentAsync(document);
        }
    }

    private static List<string?> ExtractSigners(List<DocumentSignature> signatures)
    {
        var regex = MyRegex();

        return (from signature in signatures select regex.Match(signature.DigitalCertificate?.Subject ?? string.Empty) into match where match.Success select match.Groups[1].Value).ToList();
    }
    
    private List<MetaDataDocument>? CheckMetaDataFile(string url)
    {
        if (!File.Exists(url))
        {
            _logger.LogError("File not found at {url}", url);
            return null;
        }

        var pdfReader = new PdfReader(url);
        var pdfDocument = new PdfDocument(pdfReader);

        // Read Signature Information
        var signatureUtil = new SignatureUtil(pdfDocument);
        var signatureNames = signatureUtil.GetSignatureNames();

        if (signatureNames.Count <= 0) return null;


        return (from name in signatureNames
            let signature = signatureUtil.GetSignature(name)
            let pkcs7 = signatureUtil.ReadSignatureData(name)
            select new MetaDataDocument
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
            }).ToList();

        // else
        {
            return null;
        }
    }

    [GeneratedRegex(@"CN=([^,]+)")]
    private static partial Regex MyRegex();
}