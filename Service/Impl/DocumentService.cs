using System.Net;
using AutoMapper;
using BusinessObject;
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

    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService, IWorkflowService workflowService, ILogger<DocumentService> logger, IExternalApiService externalApiService, IDigitalCertificateService digitalCertificateService, IDocumentSignatureService documentSignatureService)
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

    public async Task<ResponseDto> UploadDocument(IFormFile file,Guid userId)
    {
        var url = await _fileService.SaveFile(file);
        var metaData = CheckMetaDataFile(url);
        var aiResponse = await _externalApiService.ScanPdfAsync(url);
        var document = new Document()
        {
            DocumentName = aiResponse.DocumentName,
            DocumentContent = aiResponse.DocumentContent,
            DateIssued = aiResponse.DateIssued,
            NumberOfDocument = aiResponse.NumberOfDocument,
            //DocumentUrl = url,
            CreatedDate = DateTime.UtcNow,
            UserId = userId,
            IsDeleted = false,
            DocumentSignatures = []

        };
        await _unitOfWork.DocumentUOW.AddAsync(document);
        try{
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // _logger.LogError("Error when save document {message}",e.Message);
            return ResponseUtil.Error(e.Message, "error", HttpStatusCode.InternalServerError);
        }
        if (metaData != null)
        {
            var index = 0;
            foreach (var meta in metaData)
            {
                var certificate = (DigitalCertificate)(await _digitalCertificateService.CreateCertificate(meta, userId)).Content;
                var sig = (DocumentSignature)(await _documentSignatureService.CreateSignature(document,certificate,meta,userId,index)).Content;
                document.DocumentSignatures[index] = sig;
                await _unitOfWork.DocumentUOW.UpdateAsync(document);
                try{
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    // _logger.LogError("Error when save document {message}",e.Message);
                    return ResponseUtil.Error(e.Message, "error", HttpStatusCode.InternalServerError);
                }
                // document.DocumentSignatures.Add(signature);
                index++;
            }
        }

        var docDto = new
        {
            Name = document.DocumentName,
            document.Sender,
            document.DateReceived,
            DatePublish = document.DocumentSignatures[document.DocumentSignatures.Count-1],
            
        };
        // await _unitOfWork.SaveChangesAsync();
        return ResponseUtil.GetObject(document ,"oke",HttpStatusCode.OK,1);
    }

    public Task<ResponseDto> InsertSimpleDocument(DocumentDto document)
    {
        throw new NotImplementedException();
    }
    
    
    //*------------------------------------------------------------------*
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