using System.Linq.Dynamic;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Option;
using DataAccess;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using DataAccess.DTO.Response;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Signatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Repository;
using Service.Response;
using Service.SignalRHub;
using Service.Utilities;
using Path = System.IO.Path;

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
    private readonly INotificationService _notificationService;
    private readonly MongoDbService _notificationCollection;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly MongoDbService _mongoDbService;
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage");


    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService,
        ILogger<DocumentService> logger, IExternalApiService externalApiService,
        IDigitalCertificateService digitalCertificateService, IDocumentSignatureService documentSignatureService,
        IOptions<AppsetingOptions> options, INotificationService notificationService,
        MongoDbService notificationCollection, IHubContext<NotificationHub> hubContext, MongoDbService mongoDbService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
        _logger = logger;
        _externalApiService = externalApiService;
        _digitalCertificateService = digitalCertificateService;
        _documentSignatureService = documentSignatureService;
        _notificationService = notificationService;
        _notificationCollection = notificationCollection;
        _hubContext = hubContext;
        _mongoDbService = mongoDbService;
        _host = options.Value.Host;
    }

    #region GetAllTypeDocumentsMobile_ver1

    // public async Task<ResponseDto> GetAllTypeDocumentsMobile(Guid userId)
    // {
    //     List<AllDocumentResponseMobile> result;
    //     var cache =  await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
    //         "GetAllTypeDocumentsMobile_userId_" + userId);
    //     if (cache != null)
    //     {
    //         result = cache;
    //     }
    //     else
    //     {
    //         var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByUserId(userId);
    //         result = workflow
    //             .Select(x => new AllDocumentResponseMobile()
    //             {
    //                 WorkFlowId = x.WorkflowId,
    //                 WorkFlowName = x.WorkflowName,
    //                 DocumentTypes = x.DocumentTypeWorkflows
    //                     .Select(y => y.DocumentType)
    //                     .Select(dt => new DocumentTypeResponseMobile()
    //                     {
    //                         DocumentTypeId = dt.DocumentTypeId,
    //                         DocumentTypeName = dt.DocumentTypeName,
    //                         DocumentResponseMobiles = dt.Documents
    //                             .Where(d => !d.IsDeleted &&
    //                                         d.DocumentWorkflowStatuses.Any(dws => dws.WorkflowId == x.WorkflowId))
    //                             .Select(d => new DocumentResponseMobile()
    //                             {
    //                                 Id = d.DocumentId,
    //                                 DocumentName = d.DocumentName,
    //                                 CreatedDate = d.CreatedDate,
    //                                 Size = _fileService.GetFileSize(
    //                                     d.DocumentId,
    //                                     d.DocumentVersions.FirstOrDefault(t => t.IsFinalVersion)?.DocumentVersionId ??
    //                                     Guid.Empty,
    //                                     d.DocumentName
    //                                 )
    //                             }).ToList()
    //                     })
    //                     .Where(dtr => dtr.DocumentResponseMobiles.Any())
    //                     .ToList()
    //             })
    //             .Where(res => res.DocumentTypes.Any())
    //             .ToList();
    //         var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByUserIdAsync(userId);
    //         var tmp = new AllDocumentResponseMobile()
    //         {
    //             WorkFlowId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
    //             WorkFlowName = "Đã lưu",
    //             DocumentTypes = archiveDoc.Select(x =>
    //                 new DocumentTypeResponseMobile()
    //                 {
    //                     DocumentTypeId = x.DocumentTypeId,
    //                     DocumentTypeName = x.DocumentType.DocumentTypeName,
    //                     DocumentResponseMobiles = []
    //                 }
    //             ).Distinct().ToList()
    //         };
    //         foreach (var ad in archiveDoc)
    //         {
    //             tmp.DocumentTypes
    //                 .FirstOrDefault(x => x.DocumentTypeId == ad.DocumentTypeId)
    //                 ?.DocumentResponseMobiles?.Add(
    //                     new DocumentResponseMobile()
    //                     {
    //                         Id = ad.ArchivedDocumentId,
    //                         DocumentName = ad.ArchivedDocumentName,
    //                         CreatedDate = ad.CreatedDate,
    //                         Size = _fileService.GetFileSize(
    //                             ad.ArchivedDocumentId,
    //                             Guid.Empty,
    //                             ad.ArchivedDocumentName
    //                         )
    //                     }
    //                 );
    //         }
    //
    //         result.Add(tmp);
    //         var totalDocuments = result
    //             .SelectMany(wf => wf.DocumentTypes ?? [])
    //             .Sum(dt => dt.DocumentResponseMobiles?.Count ?? 0);
    //
    //         foreach (var wf in result)
    //         {
    //             foreach (var dt in wf.DocumentTypes ?? [])
    //             {
    //                 var count = dt.DocumentResponseMobiles?.Count ?? 0;
    //                 dt.Percent = totalDocuments > 0
    //                     ? (float)Math.Round((count * 1f) / totalDocuments, 2)
    //                     : 0;
    //             }
    //         }
    //
    //         _unitOfWork.RedisCacheUOW.SetData("GetAllTypeDocumentsMobile_userId_" + userId, result,
    //             TimeSpan.FromMinutes(3));
    //     }
    //
    //     result.ForEach(w =>
    //         w.DocumentTypes?.ForEach(dt =>
    //             dt.DocumentResponseMobiles?.Clear()
    //         )
    //     );
    //     return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    // }

    #endregion


    /// <summary>
    /// Retrieves all document types for a specific user, including workflows and archived documents.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A response containing a list of document types grouped by workflows and archived documents.</returns>
   public async Task<ResponseDto> GetAllTypeDocumentsMobile(Guid userId)
    {
        List<AllDocumentResponseMobile> result;
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
            "GetAllTypeDocumentsMobile_userId_" + userId);
        if (cache != null)
        {
            result = cache;
        }
        else
        {
            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByUserId(userId);
            result = workflow
                .Select(x => new AllDocumentResponseMobile()
                {
                    WorkFlowId = x.WorkflowId,
                    WorkFlowName = x.WorkflowName,
                    DocumentTypes = x.DocumentTypeWorkflows
                        .Select(y => y.DocumentType)
                        .Select(dt => new DocumentTypeResponseMobile()
                        {
                            DocumentTypeId = dt.DocumentTypeId,
                            DocumentTypeName = dt.DocumentTypeName,
                            DocumentResponseMobiles = dt.Documents
                                .Where(d => !d.IsDeleted &&
                                            d.DocumentWorkflowStatuses.Any(dws => dws.WorkflowId == x.WorkflowId))
                                .Select(d => new DocumentResponseMobile()
                                {
                                    Id = d.DocumentId,
                                    DocumentName = d.DocumentName,
                                    CreatedDate = d.CreatedDate,
                                    Size = _fileService.GetFileSize(
                                        d.DocumentId,
                                        d.DocumentVersions.FirstOrDefault(t => t.IsFinalVersion)?.DocumentVersionId ??
                                        Guid.Empty,
                                        d.DocumentName
                                    ) ?? "0 KB"
                                }).ToList()
                        })
                        .Where(dtr => dtr.DocumentResponseMobiles.Any())
                        .ToList()
                })
                .Where(res => res.DocumentTypes.Any())
                .ToList();
            var archiveDoc = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByUserIdAsync(userId);
            var tmp = new AllDocumentResponseMobile()
            {
                WorkFlowId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                WorkFlowName = "Đã lưu",
                DocumentTypes = archiveDoc.Select(x =>
                    new DocumentTypeResponseMobile()
                    {
                        DocumentTypeId = x.DocumentTypeId,
                        DocumentTypeName = x.DocumentType.DocumentTypeName,
                        DocumentResponseMobiles = []
                    }
                ).Distinct().ToList()
            };
            foreach (var ad in archiveDoc)
            {
                tmp.DocumentTypes
                    .FirstOrDefault(x => x.DocumentTypeId == ad.DocumentTypeId)
                    ?.DocumentResponseMobiles?.Add(
                        new DocumentResponseMobile()
                        {
                            Id = ad.ArchivedDocumentId,
                            DocumentName = ad.ArchivedDocumentName,
                            CreatedDate = ad.CreatedDate,
                            Size = _fileService.GetFileSize(
                                ad.ArchivedDocumentId,
                                Guid.Empty,
                                ad.ArchivedDocumentName
                            ) ?? "0 Kb"
                        }
                    );
            }

            result.Add(tmp);
            var totalDocuments = result
                .SelectMany(wf => wf.DocumentTypes ?? [])
                .Sum(dt => dt.DocumentResponseMobiles?.Count ?? 0);

            foreach (var wf in result)
            {
                foreach (var dt in wf.DocumentTypes ?? [])
                {
                    var count = dt.DocumentResponseMobiles?.Count ?? 0;
                    dt.Percent = totalDocuments > 0
                        ? (float)Math.Round((count * 1f) / totalDocuments, 2)
                        : 0;
                }
            }

            await _unitOfWork.RedisCacheUOW.SetDataAsync("GetAllTypeDocumentsMobile_userId_" + userId, result,
                TimeSpan.FromMinutes(3));
        }

        result.ForEach(w =>
            w.DocumentTypes?.ForEach(dt =>
                dt.DocumentResponseMobiles?.Clear()
            )
        );
        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }


    public async Task<ResponseDto> GetAllTypeDocMobile(Guid userId)
    {
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
            "GetAllTypeDocumentsMobile_userId_" + userId);
        if (cache != null)
        {
        }
        else
        {
            await GetAllTypeDocumentsMobile(userId);
            cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
                "GetAllTypeDocumentsMobile_userId_" + userId);
        }

        var result = cache.Where(w => w.DocumentTypes != null)
            .SelectMany(w => w.DocumentTypes!)
            .GroupBy(dt => dt.DocumentTypeId)
            .Select(g => new DocumentTypeResponseMobile
            {
                DocumentTypeId = g.Key,
                DocumentTypeName = g.FirstOrDefault()?.DocumentTypeName,
                Percent = g.Sum(x => x.Percent ?? 0),
                DocumentResponseMobiles = null // Bỏ luôn danh sách documents
            })
            .ToList();
        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> GetAllDocumentsByTypeMobile(Guid documentTypeId, Guid userId)
    {
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
            "GetAllTypeDocumentsMobile_userId_" + userId);
        if (cache != null)
        {
        }
        else
        {
            await GetAllTypeDocumentsMobile(userId);
            cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
                "GetAllTypeDocumentsMobile_userId_" + userId);
        }

        var result = cache.Where(w => w.DocumentTypes != null)
            .SelectMany(w => w.DocumentTypes)
            .Where(dt => dt.DocumentResponseMobiles != null)
            .SelectMany(dt => dt.DocumentResponseMobiles!,
                (dt, doc) => new { dt.DocumentTypeId, dt.DocumentTypeName, Document = doc })
            .GroupBy(x => x.DocumentTypeId)
            .Select(g =>
                g.Select(x => x.Document).ToList()
            )
            .ToList();
        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> GetDocumentByNameMobile(string documentName, Guid userId)
    {
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
            "GetAllTypeDocumentsMobile_userId_" + userId);
        if (cache != null)
        {
        }
        else
        {
            await GetAllTypeDocumentsMobile(userId);
            cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
                "GetAllTypeDocumentsMobile_userId_" + userId);
        }

        var result = cache.Where(w => w.DocumentTypes != null)
            .SelectMany(w => w.DocumentTypes!)
            .Where(dt => dt.DocumentResponseMobiles != null)
            .SelectMany(dt => dt.DocumentResponseMobiles!,
                (dt, doc) => new { dt.DocumentTypeId, dt.DocumentTypeName, Document = doc })
            .Where(x => !string.IsNullOrEmpty(x.Document.DocumentName) &&
                        x.Document.DocumentName.Contains(documentName,
                            StringComparison.OrdinalIgnoreCase)) // search tương đối
            .GroupBy(x => x.DocumentTypeId)
            .Select(g =>
                g.Select(x => x.Document).ToList()
            )
            .ToList();

        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }

    private async Task<List<SizeDocumentResponse>> GetDocumentSize(string url)
    {
        // if (!(Path.GetExtension(url) == ".docx" || Path.GetExtension(url) == ".pdf"))
        // {
        //     if (!File.Exists(url + ".pdf")) url += ".docx";
        //     else
        //     {
        //         url += ".pdf";
        //     }
        // }
        var path = String.Empty;
        if (Path.GetExtension(url) == ".docx")
        {
            path = await _fileService.ConvertDocToPdfPhysic(url);
            url = path;
        }
        else if (File.Exists(url + ".docx"))
        {
            path = await _fileService.ConvertDocToPdfPhysic(url + ".docx");
            url = path;
        }
        else
        {
            url += ".pdf";
            // path = url;
        }


        var list = new List<SizeDocumentResponse>();

        using var reader = new PdfReader(url);
        using var pdfDoc = new PdfDocument(reader);
        var numberOfPages = pdfDoc.GetNumberOfPages();
        var totalWidth = 0.0;
        var totalHeight = 0.0;
        for (var i = 1; i <= numberOfPages; i++) // i bắt đầu từ 1
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();

            var widthPt = pageSize.GetWidth();
            var heightPt = pageSize.GetHeight();

            totalWidth += widthPt;
            totalHeight += heightPt;

            list.Add(new SizeDocumentResponse
            {
                width = widthPt,
                height = heightPt,
                page = i
            });
        }

        list = list.Select(x =>
            {
                x.width = (float)(totalWidth / numberOfPages);
                x.height = (float)(totalHeight / numberOfPages);
                return x;
            }
        ).ToList();

        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            return list;
        }

        return list;
    }

    public async Task<ResponseDto> GetDocumentDetailById(Guid documentId, Guid userId)
    {
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        var task = await _unitOfWork.TaskUOW.FindTaskByDocumentIdAndUserIdAsync(documentId, userId);
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        var version = "0";
        var taskStatus = user.Tasks.OrderByDescending(t => t.TaskStatus).ToList()[0].TaskStatus;
        if (taskStatus is TasksStatus.Completed or TasksStatus.InProgress)
        {
            version = document?.DocumentVersions?.Find(t => t.IsFinalVersion)?.VersionNumber ?? "0";
        }

        var versions = document.DocumentVersions.ToList();
        var signature = document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion)?.DocumentSignatures;
        var dateExpires = DateTime.MaxValue;
        if (signature != null)
            foreach (var sig in signature.Where(sig => sig.SignedAt < dateExpires))
            {
                dateExpires = sig.SignedAt;
            }


        signature ??= [];

        var result = new DocumentResponse()
        {
            DocumentId = document.DocumentId,
            DocumentName = document.DocumentName,
            DocumentContent = document.DocumentContent,
            NumberOfDocument = document.NumberOfDocument,
            Sender = document.Sender,
            CreatedBy = document.User.FullName,
            DateReceived = document.DateReceived,
            WorkflowName = document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.WorkflowName,
            Scope = document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope.ToString(),
            Deadline = document.Deadline,
            Status = document.ProcessingStatus.ToString(),
            DocumentTypeName = document.DocumentType.DocumentTypeName,
            DateIssued = document.DateIssued,
            DateExpires = dateExpires,
            Versions = versions.Select(v => new VersionDetailRespone()
            {
                VersionNumber = v.VersionNumber,
                
                CreatedDate = v.CreateDate,
                Url = v.DocumentVersionUrl,
                IsFinal = v.IsFinalVersion
            }).ToList(),
            Tasks = task.Select(t => new TasksResponse()
            {
                TaskId = t.TaskId,
                TaskTitle = t.Title,
                Description = t.Description,
                TaskType = t.TaskType.ToString(),
                Status = t.TaskStatus.ToString()
            }).ToList(),
            Signatures = signature.Select(x => new SignatureResponse()
                {
                    SignerName = ExtractSigners(x.DigitalCertificate.Subject),
                    SignedDate = x.SignedAt,
                    IsDigital = x.DigitalCertificate.SerialNumber != null
                }
            ).ToList()
        };

        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }
    // private async Task<List<Tasks>> GetOrderedTasks(Document doc)
    // {
    //     var tasks = doc.Tasks;
    //     var workflowId = doc.DocumentWorkflowStatuses.FirstOrDefault().WorkflowId;
    //     var flowNumberMap = new Dictionary<(Guid workflowId, Guid flowId), int>();
    //
    //     async Task<int> GetFlowNumber(Guid workflowId, Guid flowId)
    //     {
    //         if (!flowNumberMap.TryGetValue((workflowId, flowId), out var number))
    //         {
    //             var wfFlow = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAndFlowIdAsync(workflowId, flowId);
    //             number = wfFlow?.FlowNumber ?? int.MaxValue;
    //             flowNumberMap[(workflowId, flowId)] = number;
    //         }
    //
    //         return number;
    //     }
    //
    //     var tasksWithFlowNumbers = new List<(Tasks Task, int FlowNumber, int StepNumber, int TaskNumber)>();
    //
    //     foreach (var t in tasks)
    //     {
    //         var flowId = t.Step?.Flow?.FlowId ?? Guid.Empty;
    //         var flowNumber = await GetFlowNumber(workflowId, flowId);
    //         var stepNumber = t.Step?.StepNumber ?? int.MaxValue;
    //         var taskNumber = t.TaskNumber;
    //
    //         tasksWithFlowNumbers.Add((t, flowNumber, stepNumber, taskNumber));
    //     }
    //
    //     var orderedTasks = tasksWithFlowNumbers
    //         .OrderBy(x => x.FlowNumber)
    //         .ThenBy(x => x.StepNumber)
    //         .ThenBy(x => x.TaskNumber)
    //         .Select(x => x.Task)
    //         .ToList();
    //
    //     // Debug: show flow info
    //     foreach (var x in orderedTasks)
    //     {
    //         var flowNumber = x.Step?.Flow?.WorkflowFlows?.FirstOrDefault(wf => wf.WorkflowId == workflowId)?.FlowNumber;
    //         Console.WriteLine($"TaskId: {x.TaskId}, FlowNumber: {flowNumber}, StepNumber: {x.Step?.StepNumber}, TaskNumber: {x.TaskNumber}");
    //     }
    //
    //     return orderedTasks;
    // }

    /// <summary>
    /// Retrieves a paginated list of documents for the specified user, applying filters and sorting based on the provided request.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="getAllMySelfRequestDto">The request object containing filter and sorting criteria.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page for pagination.</param>
    /// <returns>A response containing the filtered, sorted, and paginated list of documents.</returns>
    public async Task<ResponseDto> GetMySelfDocument(Guid userId, GetAllMySelfRequestDto getAllMySelfRequestDto, int page, int pageSize)
    {
        // Retrieve the user by their ID
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        List<DocumentJsonDto> doc;
    
        // Attempt to retrieve documents from the cache
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<DocumentJsonDto>?>(
            "GetAllMySeflDoc_userId_" + userId);
        if (cache != null)
        {
            doc = cache;
        }
        else
        {
            // Fetch documents directly from the database if not found in cache
            var tmp = (await _unitOfWork.DocumentUOW.FindAllDocumentMySelf(userId)).ToList();
    
            // Include additional documents if the user has a "Chief" role
            if (user is { UserRoles: not null } && user.UserRoles.Any(x => x.Role is { RoleName: not null } && x.Role.RoleName.Split("_")[^1] == "Chief" && x.Role.IsDeleted == false))
            {
                var docCa = await _unitOfWork.DocumentUOW.FindAllDocumentAsync();
                docCa = docCa.Where(
                    x => x.DocumentWorkflowStatuses?.FirstOrDefault()?.Workflow?.Scope == Scope.InComing);
                {
                    var li = docCa.Select(x => x.Tasks).ToList();
                    var kk = new List<Document>();
                    foreach (var t2 in li
                                 .Select(lTask =>
                                 {
                                     return lTask?.Where(x => x.UserId == userId && x is
                                     {
                                         TaskType: TaskType.Create,
                                         TaskStatus: TasksStatus.InProgress or TasksStatus.Completed,
                                         IsDeleted: false
                                     }).Select(x => x.Document).ToList();
                                 }).Select(t2 => t2?.Distinct().ToList()))
                    {
                        if (t2 != null) kk.AddRange(t2!);
                    }
    
                    kk = kk.Distinct().ToList();
                    tmp.AddRange(kk);
                }
            }
    
            // Map the documents to the DTO format
            doc = _mapper.Map<List<DocumentJsonDto>>(tmp);
        }
    
        // Transform the documents into a result list with additional details
        var result = doc.Select(x =>
        {
            var workflow = x.DocumentWorkflowStatuses?.FirstOrDefault()?.Workflow;
            if (workflow == null) return null;
            var documentSignatures = x.DocumentVersions?.FirstOrDefault(v => v.IsFinalVersion)?.DocumentSignatures;
            if (documentSignatures != null)
                return new
                {
                    Id = x.DocumentId,
                    Name = x.DocumentName,
                    CreateDate = x.CreatedDate,
                    Status = x.ProcessingStatus.ToString(),
                    Type = x.DocumentType?.DocumentTypeName ?? string.Empty,
                    Workflow = workflow?.WorkflowName,
                    Scope = workflow?.Scope.ToString(),
                    x.Deadline,
                    x.NumberOfDocument,
                    SignBy = ExtractSigners(
                        documentSignatures?.Select(c => c.DigitalCertificate)
                            .FirstOrDefault()?.Subject ?? string.Empty
                    )
                };
    
            return null;
        }).ToList();
    
        // Apply filters based on the request
        if (getAllMySelfRequestDto.Name != null) 
            result = result.Where(x => x?.Name != null && x.Name.Contains(getAllMySelfRequestDto.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (getAllMySelfRequestDto.Scope != null)
            result = result.Where(x => x?.Scope == getAllMySelfRequestDto.Scope.ToString()).ToList();
        if (getAllMySelfRequestDto.StartCreatedDate != null)
        {
            getAllMySelfRequestDto.StartCreatedDate = getAllMySelfRequestDto.StartCreatedDate.Value.AddHours(23)
                .AddMinutes(59).AddSeconds(59);
            result = result.Where(x => x?.CreateDate.CompareTo(getAllMySelfRequestDto.StartCreatedDate) >= 0).ToList();
        }
        if (getAllMySelfRequestDto.EndCreatedDate != null)
        {
            getAllMySelfRequestDto.EndCreatedDate = getAllMySelfRequestDto.EndCreatedDate.Value.AddHours(23)
                .AddMinutes(59).AddSeconds(59);

            result = result.Where(x => x?.CreateDate.CompareTo(getAllMySelfRequestDto.EndCreatedDate) <= 0).ToList();
        }
        if (getAllMySelfRequestDto.Status != null)
            result = result.Where(x => x?.Status == getAllMySelfRequestDto.Status.ToString()).ToList();
    
        // Sort the results based on the creation date
        result = getAllMySelfRequestDto.SortByCreatedDate == SortByCreatedDate.Ascending 
            ? result.OrderBy(x => x?.CreateDate).ToList() : result.OrderByDescending(x => x?.CreateDate).ToList();
    
        // Paginate the results
        var finalResult = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalPage = (int)Math.Ceiling((double)result.Count / pageSize);
    
        // Return the paginated response
        return ResponseUtil.GetCollection(finalResult, ResponseMessages.GetSuccessfully, HttpStatusCode.OK,
            result.Count, page,
            pageSize, totalPage);
    }

    public async Task<IActionResult> GetDocumentByFileName(string documentName, Guid userId)
    {
        var result = await _fileService.GetPdfFile(Path.Combine("document", "UploadedFiles", documentName));
        return result;
    }


    public async Task<ResponseDto> GetAllDocumentsMobile(Guid? workFlowId, Guid documentTypeId, Guid userId)
    {
        List<DocumentResponseMobile> result;
        var cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
            "GetAllTypeDocumentsMobile_userId_" + userId);
        if (cache != null)
        {
            result = cache
                .Where(wf => wf.WorkFlowId == workFlowId)
                .SelectMany(wf => wf.DocumentTypes ?? [])
                .Where(dt => dt.DocumentTypeId == documentTypeId)
                .SelectMany(dt => dt.DocumentResponseMobiles ?? [])
                .ToList();
        }
        else
        {
            await GetAllTypeDocumentsMobile(userId);
            cache = await _unitOfWork.RedisCacheUOW.GetDataAsync<List<AllDocumentResponseMobile>>(
                "GetAllTypeDocumentsMobile_userId_" + userId);
            result = cache
                .Where(wf => wf.WorkFlowId == workFlowId)
                .SelectMany(wf => wf.DocumentTypes ?? [])
                .Where(dt => dt.DocumentTypeId == documentTypeId)
                .SelectMany(dt => dt.DocumentResponseMobiles ?? [])
                .ToList();
        }

        return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> GetDocumentDetailByIdMobile(Guid? documentId, Guid userId, Guid workFlowId)
    {
        var isArchive = workFlowId.Equals(Guid.Parse("00000000-0000-0000-0000-000000000000"));
        if (!isArchive)
        {
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);

            var users = new List<User> { document.User };
            if (document.DocumentVersions.FirstOrDefault(t => t.IsFinalVersion) != null)
                users.AddRange(document.DocumentVersions.FirstOrDefault(t => t.IsFinalVersion).DocumentSignatures
                    .Select(t => t.DigitalCertificate.User)
                    .ToList());
            users.AddRange(document.Tasks.Select(x => x.User).ToList());
            users = users.Distinct().ToList();
            users = users.Where(u => u != null).ToList();
            var divisions = users.Select(u => u?.Division?.DivisionName).Distinct().ToList();
            var userList = users.Select(u => new UserResponseMobile()
            {
                UserId = u.UserId,
                FullName = u.UserName,
                DivisionName = u.Division.DivisionName
            }).ToList();
            var version = "0";
            var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
            var taskStatus = user.Tasks.OrderByDescending(x => x.TaskStatus).ToList()[0].TaskStatus;
            if (taskStatus is TasksStatus.Completed or TasksStatus.InProgress)
                version = document.DocumentVersions.FirstOrDefault(t => t.IsFinalVersion)?.VersionNumber ?? "0";
            var v = document.DocumentVersions.FirstOrDefault(x => x.VersionNumber == version);

            var sizes = await GetDocumentSize(Path.Combine(Directory.GetCurrentDirectory(), "data", "storage",
                "document",
                documentId.ToString(), v.DocumentVersionId.ToString(), document.DocumentName));
            var result = new DocumentDetailResponse()
            {
                Sizes = sizes,
                DocumentId = document.DocumentId,
                DocumentName = document.DocumentName,
                DocumentContent = document.DocumentContent,
                NumberOfDocument = document.NumberOfDocument,
                ProcessingStatus = document.ProcessingStatus,
                DateIssued = document.DateIssued,
                DocumentTypeName = document.DocumentType.DocumentTypeName,
                CreatedDate = document.CreatedDate,
                CreatedBy = document.User.UserName,
                DivisionList = divisions,
                UserList = userList,
                SignBys = ExtractSigners(document.DocumentVersions.FirstOrDefault(x => x.VersionNumber == version)
                    .DocumentSignatures),
                DocumentUrl = document.DocumentVersions.FirstOrDefault(x => x.VersionNumber == version)
                    .DocumentVersionUrl
            };
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }

        var documentA = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(documentId);
        var usersA = documentA.UserDocumentPermissions.Select(x => x.User).Distinct().ToList();

        var divisionsA = usersA.Select(u => u?.Division?.DivisionName).Distinct().ToList();
        var userListA = usersA.Select(u => new UserResponseMobile()
        {
            UserId = u.UserId,
            FullName = u.UserName,
            DivisionName = u.Division.DivisionName
        }).ToList();
        var resultA = new DocumentDetailResponse()
        {
            Sizes = await GetDocumentSize(Path.Combine(Directory.GetCurrentDirectory(), "data", "storage",
                "archive_document", documentA.ArchivedDocumentId.ToString(), documentA.ArchivedDocumentName)),

            DocumentId = documentA.ArchivedDocumentId,
            DocumentName = documentA.ArchivedDocumentName,
            DocumentContent = documentA.ArchivedDocumentContent,
            NumberOfDocument = documentA.NumberOfDocument,
            ProcessingStatus = 0,
            DateIssued = documentA.DateIssued,
            DocumentTypeName = documentA.DocumentType.DocumentTypeName,
            CreatedDate = documentA.CreatedDate,
            CreatedBy = documentA.CreatedBy,
            DivisionList = divisionsA,
            UserList = userListA,
            SignBys = ExtractSigners(documentA.ArchiveDocumentSignatures),
            DocumentUrl = documentA.ArchivedDocumentUrl
        };
        return ResponseUtil.GetObject(resultA, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> ClearCacheDocumentMobile(Guid userId)
    {
        await _unitOfWork.RedisCacheUOW.RemoveDataAsync("GetAllTypeDocumentsMobile_userId_" + userId);
        return ResponseUtil.GetObject("oke", "oke", HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> CreateIncomingDoc(DocumentUploadDto documentUploadDto, Guid userId)
    {
        var name = GetString(documentUploadDto.CanChange.GetValueOrDefault("Name"));
        var sender = GetString(documentUploadDto.CanChange.GetValueOrDefault("Sender"));
        var numberOfDocument = GetString(documentUploadDto.CanChange.GetValueOrDefault("NumberOfDocument"));
        var documentContent = GetString(documentUploadDto.CanChange.GetValueOrDefault("DocumentContent"));
        var dateReceived = GetDateTime(documentUploadDto.CanChange.GetValueOrDefault("DateReceived"));
        var documentTypeId = GetGuid(documentUploadDto.CanChange.GetValueOrDefault("DocumentTypeId"));
        var workflowId = GetGuid(documentUploadDto.CanChange.GetValueOrDefault("WorkflowId"));
        var deadline = GetDateTime(documentUploadDto.CanChange.GetValueOrDefault("Deadline")) ?? DateTime.Now;
        var validFrom = GetDateTime(documentUploadDto.CanChange.GetValueOrDefault("ValidFrom")) ?? DateTime.Now;
        var workflowO = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
        var workflowFlow = workflowO.WorkflowFlows.Select(x => x).FirstOrDefault(x => x.FlowNumber == 1);

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
            DateIssued = validFrom,
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
            VersionNumber = "0",
            CreateDate = DateTime.Now,
            IsFinalVersion = true
        };
        document.DocumentVersions.Add(version);
        await _unitOfWork.DocumentUOW.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        var url = _fileService.CreateAVersionFromUpload(documentUploadDto.CannotChange["fileName"]?.ToString(),
            version.DocumentVersionId, document.DocumentId, name);
        version.DocumentVersionUrl = url;

        // document.DateIssued = DateTime.Today.ToString("yyyy-MM-dd");
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

        Guid? GetGuid(object? obj)
        {
            return obj is JsonElement e && Guid.TryParse(e.GetString(), out var guid) ? guid : null;
        }

        DateTime? GetDateTime(object? obj)
        {
            return obj is JsonElement e && DateTime.TryParse(e.GetString(), out var dt) ? dt : null;
        }

        string? GetString(object? obj)
        {
            return obj is JsonElement e ? e.GetString() : obj as string;
        }
    }

    public async Task<IActionResult> GetDocumentById(Guid documentId, string version, bool isDoc)
    {
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        var versionId1 = document.DocumentVersions.FirstOrDefault(x => x.VersionNumber == version)?.DocumentVersionId;
        var result = await _fileService.GetPdfFile(Path.Combine("document", documentId.ToString(),
            versionId1.ToString(),
            document.DocumentName + (isDoc ? ".docx" : ".pdf")));

        return result;
    }

    public async Task<ResponseDto> UpdateConfirmTaskWithDocument(Guid documentId)
    {
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        var version = document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion);
        switch (document.DocumentWorkflowStatuses.FirstOrDefault().Workflow.Scope)
        {
            case Scope.InComing:
                var archiveId = Guid.NewGuid();
                var taskList = document.Tasks;
                var li = new List<UserDocumentPermission>();
                foreach (var task in taskList)
                {
                    var permision = new UserDocumentPermission()
                    {
                        CreatedDate = DateTime.Now,
                        IsDeleted = false,
                        UserId = task.UserId,
                    };
                    li.Add(permision);
                    var user = await _unitOfWork.UserUOW.FindUserByIdAsync(task.UserId);
                    var notification = _notificationService.CreateTaskAssignNotification(task, task.UserId);
                    await _notificationCollection.CreateNotificationAsync(notification);
                    await _notificationService.SendPushNotificationMobileAsync(user.FcmToken, notification);
                    await _hubContext.Clients.User(notification.UserId.ToString())
                        .SendAsync("ReceiveMessage", notification);
                }

                document.FinalArchiveDocumentId = archiveId;
                var archiveDocument = new ArchivedDocument
                {
                    UserDocumentPermissions = li,
                    ArchivedDocumentId = archiveId,
                    ArchivedDocumentName = document.DocumentName,
                    ArchivedDocumentContent = document.DocumentContent,
                    NumberOfDocument = document.NumberOfDocument,
                    CreatedDate = DateTime.Now,
                    Sender = document.Sender,
                    CreatedBy = document.User.UserName,
                    ExternalPartner = document.Sender,
                    ArchivedDocumentStatus = ArchivedDocumentStatus.Archived,
                    DateIssued = document.DateIssued,
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
            default:
                return ResponseUtil.Error("Invalid Workflow Scope", "Operation Failed", HttpStatusCode.BadRequest);
        }

        return ResponseUtil.GetObject("oke", "Success", HttpStatusCode.OK, 1);
    }

    public async Task<IActionResult> GetArchiveDocumentById(Guid documentId, string? version)
    {
        var document = await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(documentId);
        var result = await _fileService.GetPdfFile(Path.Combine("archive_document", documentId.ToString(),
            document.ArchivedDocumentName + ".pdf"));
        return result;
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
            { "NewSignerName", null },
            { "validTo", metaData?.MaxBy(x => x.ExpirationDate).ExpirationDate },
            { "validFrom", metaData?.MinBy(x => x.ValidFrom).ValidFrom },
            { "signerName", metaData?.Select(x => ExtractSigners(x.SignerName)).ToList() },
            { "url", _host + "/api/document/view-file-by-name?documentName=" + fileName }
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

    private static List<string> ExtractSigners(List<ArchiveDocumentSignature> signatures)
    {
        var regex = MyRegex();
        return signatures
            .Select(signature => regex.Match(signature.DigitalCertificate?.Subject ?? string.Empty))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .ToList();
    }

    public void AddFooterToPdf(string inputFilePath, string outputFilePath)
    {
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        using (var pdfReader = new PdfReader(inputFilePath))
        using (var pdfWriter = new PdfWriter(outputFilePath))
        using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
        {
            var document = new iText.Layout.Document(pdfDocument);

            // Define the footer text
            var footerText = new Paragraph("Copyright by DMS")
                .SetFontSize(6)
                .SetFontColor(new DeviceRgb(0xb4, 0xb6, 0xb8))
                .SetTextAlignment(TextAlignment.CENTER);

            // Add the footer to each page
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var pageSize = pdfDocument.GetPage(i).GetPageSize();
                float x = pageSize.GetWidth() / 2;
                float y = pageSize.GetBottom() + 20; // Adjust the Y position as needed
                document.ShowTextAligned(footerText, x, y, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
            }
        }

        Console.WriteLine($"Footer added. Modified file saved at: {outputFilePath}");
    }

    public async Task<ResponseDto> CreateDocumentByTemplate(DocumentPreInfo documentPreInfo, Guid userId)
    {
        var docId = Guid.NewGuid();
        var filter = Builders<Count>.Filter.Eq(x => x.Id, "base");
        var count = _mongoDbService.Counts.Find(filter).FirstOrDefault();
        var documentType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByIdAsync(documentPreInfo.DocumentTypeId);
        count.Value += 1;
        var update = Builders<Count>.Update.Set(x => x.Value, count.Value);
        await _mongoDbService.Counts.UpdateOneAsync(filter, update);
        var versionId = Guid.NewGuid();
        _fileService.CreateFirstVersion(docId, documentPreInfo.DocumentName, versionId, documentPreInfo.TemplateId);
        var url = (await _unitOfWork.ArchivedDocumentUOW.FindArchivedDocumentByIdAsync(documentPreInfo.TemplateId))
            .ArchivedDocumentUrl + "&isPdf=true";
        var doc = new Document()
        {
            DocumentId = docId,
            DocumentName = documentPreInfo.DocumentName,
            NumberOfDocument = (count.Value < 10 ? "0" + count.Value : count.Value) + "/" + count.UpdateTime.Year +
                               "/" + documentType.Acronym + "-TNABC",
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
            Deadline = documentPreInfo.Deadline,
            ProcessingStatus = ProcessingStatus.InProgress,
            DocumentPriority = DocumentPriority.High,
            IsDeleted = false,
            UserId = userId,
            TemplateArchiveDocumentId = documentPreInfo.TemplateId,
            DocumentTypeId = documentPreInfo.DocumentTypeId,
            DocumentVersions =
            [
                new DocumentVersion()
                {
                    DocumentVersionId = versionId,
                    VersionNumber = "0",
                    CreateDate = DateTime.Now,
                    IsFinalVersion = true,
                    DocumentVersionUrl = url
                }
            ]
        };
        var workFlowFlow =
            (await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(documentPreInfo.WorkFlowId)).WorkflowFlows
            .FirstOrDefault(x => x.FlowNumber == 1);
        var workflowStatus = new DocumentWorkflowStatus()
        {
            StatusDocWorkflow = StatusDocWorkflow.Pending,
            StatusDoc = StatusDoc.Pending,
            UpdatedAt = DateTime.Now,
            DocumentId = docId,
            WorkflowId = documentPreInfo.WorkFlowId,
            CurrentWorkflowFlow = workFlowFlow
        };
        await _unitOfWork.DocumentWorkflowStatusUOW.AddAsync(workflowStatus);
        await _unitOfWork.DocumentUOW.AddAsync(doc);
        await _unitOfWork.SaveChangesAsync();

        return ResponseUtil.GetObject(docId, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
        // throw new NotImplementedException();
    }

    public async Task<ResponseDto> UploadDocumentForSumit(DocumentUpload documentUpload, Guid userId)
    {
        var doc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentUpload.DocumentId);
        // if (doc.ProcessingStatus != ProcessingStatus.Rejected && doc.DocumentVersions.Count > 1)
        //     return ResponseUtil.Error("File đã được gửi lên trước đó", ResponseMessages.FailedToSaveData,
        //         HttpStatusCode.BadRequest);
        var template = doc.TemplateArchiveDocument;
        var aiResponse = new DocumentAiResponse();
        var url = string.Empty;
        if (Path.GetExtension(documentUpload.File.FileName) != ".docx")
            return ResponseUtil.Error("File không phải là mẫu", ResponseMessages.FailedToSaveData,
                HttpStatusCode.BadRequest);
        try
        {
            url = await _fileService.InsertNumberDocument(documentUpload.File,
                doc.TemplateArchiveDocumentId ?? Guid.Empty,
                doc.NumberOfDocument, template.Page ?? 1, template.Llx ?? 0, template.Lly ?? 0, template.Urx ?? 0,
                template.Ury ?? 0);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.FailedToSaveData, HttpStatusCode.BadRequest);
        }

        aiResponse = await _externalApiService.ScanPdfAsync(url);
        var base64 = Convert.ToBase64String(await File.ReadAllBytesAsync(url));
        var isDifferent = doc.DocumentName != aiResponse.DocumentName ||
                          doc.DocumentType.DocumentTypeName != aiResponse.DocumentType;
        var result = new DocumentCompareDto()
        {
            DocumentId = doc.DocumentId,
            DocumentName = doc.DocumentName,
            DocumentTypeName = doc.DocumentType.DocumentTypeName,
            AiDocumentName = aiResponse.DocumentName,
            AiDocumentType = aiResponse.DocumentType,
            DocumentContent = aiResponse.DocumentContent,
            NumberOfDocument = doc.NumberOfDocument,
            IsDifferent = isDifferent,
            FileBase64 = base64,
        };
        File.Delete(url);

        return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> UpdateConfirmDocumentBySubmit(DocumentCompareDto documentUpload, Guid userId)
    {
        var doc = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentUpload.DocumentId);
        // var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        if (doc == null)
        {
            return ResponseUtil.Error("Document not found", ResponseMessages.OperationFailed,
                HttpStatusCode.NotFound);
        }
        
        // var taskList = doc.Tasks;
        // if (taskList != null)
        // {
        //     var task = taskList.FindAll(x => x.Step is { StepNumber: 1 }).FindAll(x => x is { TaskNumber: 2, TaskType: TaskType.Upload });
        //     if (task.Count == 0)
        //     {
        //         return ResponseUtil.Error("Task 2 step 1 must be upload", ResponseMessages.OperationFailed,
        //             HttpStatusCode.NotFound);
        //     }
        // }

        doc.DocumentContent = documentUpload.DocumentContent;

        var versionId = Guid.NewGuid();
        var versionMax = int.Parse(doc.DocumentVersions.OrderByDescending(x => x.VersionNumber).First().VersionNumber);
        // if (versionMax != 0 && doc.ProcessingStatus != ProcessingStatus.Rejected)
        //     return ResponseUtil.Error("File đã được gửi lên trước đó", ResponseMessages.FailedToSaveData,
        //         HttpStatusCode.BadRequest);

        foreach (var ver in doc.DocumentVersions.Where(ver => ver.IsFinalVersion))
        {
            ver.IsFinalVersion = false;
            await _unitOfWork.DocumentVersionUOW.UpdateAsync(ver);
        }

        var versionNow = new DocumentVersion()
        {
            DocumentVersionId = versionId,
            VersionNumber = (versionMax + 1).ToString(),
            CreateDate = DateTime.Now,
            IsFinalVersion = true,
            DocumentId = doc.DocumentId
        };
        var url = await _fileService.SaveNewVersionDocFromBase64(documentUpload, versionNow);
        versionNow.DocumentVersionUrl = url;
        // doc.DocumentVersions.Add(versionNow);
        await _unitOfWork.DocumentVersionUOW.AddAsync(versionNow);
        await _unitOfWork.SaveChangesAsync();
        return ResponseUtil.GetObject(url, ResponseMessages.CreatedSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> GetDocumentForUsb(Guid documentId, Guid parse)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(parse);
        string resultFile;
        string resultCertificate;

        #region Handle file to Base 64

        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        if (document != null)
        {
            if (document.DocumentVersions != null)
            {
                var version = document.DocumentVersions.FirstOrDefault(t => t.IsFinalVersion);
                if (version != null)
                {
                    var fileByte = await
                        _fileService.GetFileBytes(
                            $"document/{documentId}/{version.DocumentVersionId}/{document.DocumentName}.pdf");
                    resultFile = Convert.ToBase64String(fileByte.FileBytes);
                }
                else
                {
                    return ResponseUtil.Error("Version latest not found", ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                }
            }
            else
            {
                return ResponseUtil.Error("Dont have any version", ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            }
        }
        else
        {
            return ResponseUtil.Error("Document not found", ResponseMessages.OperationFailed,
                HttpStatusCode.NotFound);
        }

        #endregion

        #region Handle digital certificate user

        if (user != null)
        {
            if (user.DigitalCertificates != null)
            {
                var certificate = user.DigitalCertificates.FirstOrDefault(x => x.IsUsb == true);
                if (certificate != null)
                {
                    var fileByte = await _fileService.GetFileBytes(
                        $"signature/{certificate.DigitalCertificateId}.png");
                    resultCertificate = Convert.ToBase64String(fileByte.FileBytes);
                }
                else
                {
                    return ResponseUtil.Error("Certificate not found", ResponseMessages.OperationFailed,
                        HttpStatusCode.NotFound);
                }
            }
            else
            {
                return ResponseUtil.Error("Certificate not found", ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            }
        }
        else
        {
            return ResponseUtil.Error("User not found", ResponseMessages.OperationFailed,
                HttpStatusCode.NotFound);
        }

        #endregion

        #region Handle result

        var result = new DocumentForSignByUsb()
        {
            File = resultFile,
            Image = resultCertificate
        };

        #endregion

        return ResponseUtil.GetObject(result,
            ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
    }

    public async Task<ResponseDto> UpdateDocumentFromUsb(DocumentForSignByUsb documentForSignByUsb, Guid documentId,
        Guid userId)
    {
        var user = await _unitOfWork.UserUOW.FindUserByIdAsync(userId);
        var documentBase64 = documentForSignByUsb.File;
        var documentByte = Convert.FromBase64String(documentBase64);
        var pathTmp = Path.Combine(_storagePath, "tmp", Guid.NewGuid() + ".pdf");
        await File.WriteAllBytesAsync(pathTmp, documentByte);
        var metaData = CheckMetaDataFile(pathTmp);
        var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
        if (document != null)
        {
            if (document.DocumentVersions != null)
            {
                var version = document.DocumentVersions.FirstOrDefault(x => x.IsFinalVersion);
                if (version != null)
                {
                    // if (metaData?.Any(meta => !meta.IsValid) == true)
                    //     return ResponseUtil.Error("null", "Signature is not valid", HttpStatusCode.BadRequest);
                    if (user != null)
                    {
                        if (user.DigitalCertificates != null)
                        {
                            var cer = user.DigitalCertificates.FirstOrDefault(x => x.IsUsb == true);
                            if (cer is { SerialNumber: null })
                            {
                                if (metaData != null)
                                {
                                    cer.SerialNumber = metaData[^1].SerialNumber;
                                    cer.Issuer = metaData[^1].Issuer;
                                    cer.Subject = metaData[^1].SignerName;
                                    cer.ValidFrom = metaData[^1].ValidFrom;
                                    cer.ValidTo = metaData[^1].ExpirationDate;
                                    await _unitOfWork.DigitalCertificateUOW.UpdateAsync(cer);
                                    File.Replace(pathTmp,
                                        Path.Combine(_storagePath, "document", documentId.ToString(),
                                            version.DocumentVersionId.ToString(), document.DocumentName + ".pdf"),
                                        null);
                                    await _unitOfWork.SaveChangesAsync();
                                    return ResponseUtil.GetObject("Sign success",
                                        ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
                                }
                                else
                                {
                                    // return ResponseUtil.Error("Sign not success", ResponseMessages.OperationFailed,
                                    //     HttpStatusCode.NotFound);
                                    throw new Exception("Sign not success");
                                }
                            }
                            else
                            {
                                if (cer != null && metaData != null && cer.SerialNumber != metaData[^1].SerialNumber)
                                {
                                    // return ResponseUtil.Error("Wrong certificate", ResponseMessages.OperationFailed,
                                    //     HttpStatusCode.NotFound);
                                    throw new Exception("Wrong certificate");
                                }

                                File.Replace(pathTmp,
                                    Path.Combine(_storagePath, "document", documentId.ToString(),
                                        version.DocumentVersionId.ToString(), document.DocumentName + ".pdf"), null);
                                return ResponseUtil.GetObject("Sign success",
                                    ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
                            }
                        }
                        else
                        {
                            // return ResponseUtil.Error("Certificate not found", ResponseMessages.OperationFailed,
                            //     HttpStatusCode.NotFound);
                            throw new Exception("Certificate not found");
                        }
                    }
                    else
                    {
                        // return ResponseUtil.Error("User not found", ResponseMessages.OperationFailed,
                        //     HttpStatusCode.NotFound);
                        throw new Exception("User not found");
                    }
                }
                else
                {
                    // return ResponseUtil.Error("Not have version active", ResponseMessages.OperationFailed,
                    //     HttpStatusCode.NotFound);
                    throw new Exception("Not have version active");
                }
            }
            else
            {
                // return ResponseUtil.Error("Version not found", ResponseMessages.OperationFailed,
                //     HttpStatusCode.NotFound);
                throw new Exception("Version not found");
            }
        }
        else
        {
            // return ResponseUtil.Error("Document not found", ResponseMessages.OperationFailed,
            //     HttpStatusCode.NotFound);
            throw new Exception("Document not found");
        }

        // throw new NotImplementedException();
    }

    private static DateTime ParsePdfDate(string pdfDate)
    {
        // Bỏ tiền tố "D:"
        if (pdfDate.StartsWith("D:"))
            pdfDate = pdfDate.Substring(2);

        // Tách phần datetime và timezone
        string dateTimePart = pdfDate;
        string timeZonePart = "Z"; // mặc định UTC

        var tzIndex = pdfDate.IndexOfAny(new[] { '+', '-', 'Z' });
        if (tzIndex >= 0)
        {
            dateTimePart = pdfDate.Substring(0, tzIndex);
            timeZonePart = pdfDate.Substring(tzIndex + 1).Replace("'", "");
        }

        // Parse datetime cơ bản
        var dt = DateTime.ParseExact(dateTimePart, "yyyyMMddHHmmss", null);

        // Xử lý timezone nếu có
        if (timeZonePart != "Z")
        {
            // Giờ lệch so với UTC
            TimeSpan offset = TimeSpan.ParseExact(timeZonePart, "hhmm", null);
            if (timeZonePart.StartsWith("-"))
                offset = -offset;

            // Gán thông tin timezone
            var offsetTime = new DateTimeOffset(dt, offset);
            return offsetTime.UtcDateTime.ToLocalTime(); // hoặc .DateTime nếu không cần UTC
        }

        return dt;
    }

    public List<MetaDataDocument>? CheckMetaDataFile(string url)
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
                    ValidFrom = ParsePdfDate(signature.GetDate().ToString()),
                    ExpirationDate = pkcs7.GetSigningCertificate().GetNotAfter().ToLocalTime(),
                    Algorithm = pkcs7.GetSignatureAlgorithmName()
                };
            }).ToList()
            : null;
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


    public async Task<ResponseDto> ShowProcessDocumentDetail(Guid? documentId)
    {
        try
        {
            var document = await _unitOfWork.DocumentUOW.FindDocumentByIdAsync(documentId);
            if (document == null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            }

            var documentDetailAllWorkflowResponse = new DocumentDetailAllWorkflowResponse();
            documentDetailAllWorkflowResponse.DocumentId = document.DocumentId;
            documentDetailAllWorkflowResponse.DocumentName = document.DocumentName;
            documentDetailAllWorkflowResponse.DocumentTypeName = document.DocumentType.DocumentTypeName;
            // var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(document.DocumentWorkflowStatuses.FirstOrDefault().WorkflowId);
            // if (workflow == null)
            //     return ResponseUtil.Error(ResponseMessages.WorkflowNotFound, ResponseMessages.OperationFailed,
            //         HttpStatusCode.NotFound);
            // var workflowFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflow.WorkflowId);
            // var workflowFlowIds = workflowFlows.Select(wf => wf.WorkflowFlowId).ToList();
            var workflowId = document.DocumentWorkflowStatuses.FirstOrDefault().WorkflowId;
            if (workflowId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.WorkflowIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var workflow = await _unitOfWork.WorkflowUOW.FindWorkflowByIdAsync(workflowId);
            if (workflow == null)
                return ResponseUtil.Error(ResponseMessages.WorkflowNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            var workflowFlows = await _unitOfWork.WorkflowFlowUOW.FindWorkflowFlowByWorkflowIdAsync(workflowId);
            var workflowFlowIds = workflowFlows.Select(wf => wf.WorkflowFlowId).ToList();

            var transitions = await _unitOfWork.WorkflowFlowTransitionUOW.FindByWorkflowFlowIdsAsync(workflowFlowIds);

            var flowIds = workflowFlows.Select(wf => wf.FlowId).ToList();
            var flows = await _unitOfWork.FlowUOW.FindByIdsAsync(flowIds);

            var steps = await _unitOfWork.StepUOW.FindByFlowIdsAsync(flowIds);

            // var flowDtoList = workflowFlows
            //     .OrderBy(wf => wf.FlowNumber) // 🔹 Thêm dòng này để sắp xếp
            //     .Select(workflowFlow =>
            //     {
            //         // var isFallbackFlow = transitions.Any(t =>
            //         //     t.NextWorkFlowFlowId == workflowFlow.WorkflowFlowId &&
            //         //     t.Condition == FlowTransitionCondition.Reject
            //         // );
            //
            //         var flow = flows.FirstOrDefault(f => f.FlowId == workflowFlow.FlowId);
            //         if (flow == null) return null;
            //
            //         var stepsInFlow = steps.Where(s => s.FlowId == flow.FlowId).ToList();
            //         var stepIds = stepsInFlow.Select(s => s.StepId).ToList();
            //         var tasks = await _unitOfWork.TaskUOW.FindTasksByStepIdsAsync(stepIds);
            //         var taskDtos = _mapper.Map<List<TaskDto>>(tasks);
            //         return new FlowDto
            //         {
            //             FlowId = flow.FlowId,
            //             IsFallbackFlow = false,
            //             RoleStart = flow.RoleStart,
            //             RoleEnd = flow.RoleEnd,
            //             Steps = stepsInFlow
            //                 .OrderBy(s => s.StepNumber) // 🔹 Nếu cần sắp xếp steps theo StepNumber
            //                 .Select(s => new StepDto
            //                 {
            //                     StepId = s.StepId,
            //                     StepNumber = s.StepNumber,
            //                     Action = s.Action,
            //                     RoleId = s.RoleId,
            //                     Role = new RoleDto
            //                     {
            //                         RoleId = s.RoleId,
            //                         RoleName = s.Role?.RoleName,
            //                         CreatedDate = s.Role?.CreatedDate
            //                     },
            //                     NextStepId = s.NextStepId,
            //                     RejectStepId = s.RejectStepId,
            //                     IsFallbackStep = s.RejectStepId == null, // ✅ Gán giá trị ở đây thay vì sửa trong entity
            //                     TaskDtos = taskDtos // Nếu có TaskId thì lấy từ bảng liên quan
            //                 }).ToList()
            //         };
            //     })
            //     .Where(flowDto => flowDto != null) // ✅ Loại bỏ flow null
            //     .ToList();

            var flowDtoList = new List<FlowDto>();

            foreach (var workflowFlow in workflowFlows.OrderBy(wf => wf.FlowNumber))
            {
                var flow = flows.FirstOrDefault(f => f.FlowId == workflowFlow.FlowId);
                if (flow == null) continue;

                var stepsInFlow = steps.Where(s => s.FlowId == flow.FlowId).ToList();
                var stepIdsInFlow = stepsInFlow.Select(s => s.StepId).ToList();

                var tasksInFlow = await _unitOfWork.TaskUOW.FindTasksByStepIdsAsync(stepIdsInFlow, documentId);
                var taskDtosInFlow = _mapper.Map<List<TaskDto>>(tasksInFlow);

                var stepDtos = stepsInFlow
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new StepDto
                    {
                        StepId = s.StepId,
                        StepNumber = s.StepNumber,
                        Action = s.Action,
                        RoleId = s.RoleId,
                        Role = new RoleDto
                        {
                            RoleId = s.RoleId,
                            RoleName = s.Role?.RoleName,
                            CreatedDate = s.Role?.CreatedDate
                        },
                        NextStepId = s.NextStepId,
                        RejectStepId = s.RejectStepId,
                        IsFallbackStep = s.RejectStepId == null,
                        TaskDtos = taskDtosInFlow.Where(t => t.StepId == s.StepId).OrderBy(t => t.TaskNumber).ToList()
                    }).ToList();

                var flowDto = new FlowDto
                {
                    FlowId = flow.FlowId,
                    IsFallbackFlow = false,
                    RoleStart = flow.RoleStart,
                    RoleEnd = flow.RoleEnd,
                    Steps = stepDtos
                };

                flowDtoList.Add(flowDto);
            }


            var workflowResponse = new WorkflowRequest
            {
                WorkflowId = workflow.WorkflowId,
                WorkflowName = workflow.WorkflowName,
                RequiredRolesJson = workflow.RequiredRolesJson,
                Scope = workflow.Scope,
                IsAllocate = workflow.IsAllocate,
                CreateBy = workflow.CreateBy ?? Guid.Empty,
                Flows = flowDtoList
            };

            documentDetailAllWorkflowResponse.WorkflowRequest = workflowResponse;

            return ResponseUtil.GetObject(documentDetailAllWorkflowResponse,
                ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
}