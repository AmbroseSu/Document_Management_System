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

public class DocumentService : IDocumentService
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
        throw new NotImplementedException();
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

        // Scan the PDF using an external API to extract AI-generated information
        var aiResponse = await _externalApiService.ScanPdfAsync(url);

        // Find the document type based on the AI response
        var docType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByNameAsync(aiResponse.DocumentType);

        // Create a new document object with the extracted information
        var document = new Document
        {
            DocumentName = aiResponse.DocumentName,
            DocumentContent = aiResponse.DocumentContent,
            DateIssued = aiResponse.DateIssued,
            NumberOfDocument = aiResponse.NumberOfDocument,
            //DocumentUrl = url,
            CreatedDate = DateTime.UtcNow,
            UserId = userId,
            IsDeleted = false,
            //DocumentSignatures = [], // Initialize an empty list for document signatures
            DocumentType = docType
        };

        // Save the document to the database
        await SaveDocumentAsync(document);

        // If metadata is found, process it and associate it with the document
        if (metaData != null)
        {
            await ProcessMetaDataAsync(metaData, document, userId);
        }

        // Extract the names of the signers from the document signatures
        //var signBy = ExtractSigners(document.DocumentSignatures);

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
            //SignBy = signBy, // List of signers
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
        for (int index = 0; index < metaData.Count; index++)
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

    private List<string?> ExtractSigners(List<DocumentSignature> signatures)
    {
        var regex = new Regex(@"CN=([^,]+)");
        var signBy = new List<string?>();

        foreach (var signature in signatures)
        {
            var match = regex.Match(signature.DigitalCertificate?.Subject ?? string.Empty);
            if (match.Success)
            {
                signBy.Add(match.Groups[1].Value);
            }
        }

        return signBy;
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

        if (signatureNames.Count > 0)
        {
            var listMetaData = new List<MetaDataDocument>();
            foreach (var name in signatureNames)
            {
                var signature = signatureUtil.GetSignature(name);
                var pkcs7 = signatureUtil.ReadSignatureData(name);

                listMetaData.Add(new MetaDataDocument
                {
                    SignatureName = name,
                    SignerName = pkcs7.GetSigningCertificate().GetSubjectDN().ToString(),
                    SingingDate = pkcs7.GetSignDate(),
                    Reason = signature.GetReason(),
                    Location = signature.GetLocation(),
                    Valid = pkcs7.VerifySignatureIntegrityAndAuthenticity(),
                    SerialNumber = pkcs7.GetSigningCertificate().GetSerialNumber().ToString(),
                    ValidFrom = pkcs7.GetSigningCertificate().GetNotBefore().ToLocalTime(),
                    ExpirationDate = pkcs7.GetSigningCertificate().GetNotAfter().ToLocalTime(),
                    Algorithm = pkcs7.GetSignatureAlgorithmName()
                });
            }


            return listMetaData;
        }

        // else
        {
            return null;
        }
    }
}