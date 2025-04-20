using System.Net;
using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using Repository;
using Service.Response;
using Service.Utilities;

namespace Service.Impl;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentTypeService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResponseDto> AddDocumentTypeAsync(DocumentTypeDto documentTypeDto)
    {
        try
        {
            if (documentTypeDto.DocumentTypeName == null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNameNotNull, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            
            var documentTypeWithName = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByNameAsync(documentTypeDto.DocumentTypeName);
            if (documentTypeWithName != null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNameExisted, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            var documentType = new DocumentType
            {
                DocumentTypeName = documentTypeDto.DocumentTypeName,
                Acronym = documentTypeDto.Acronym,
                IsDeleted = false
            };
            //var division = _mapper.Map<Division>(divisionDto);
            await _unitOfWork.DocumentTypeUOW.AddAsync(documentType);
            await _unitOfWork.SaveChangesAsync();
            var result = _mapper.Map<DocumentTypeDto>(documentType);
            return ResponseUtil.GetObject(result, ResponseMessages.CreatedSuccessfully, HttpStatusCode.Created, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> UpdateDocumentTypeAsync(DocumentTypeDto documentTypeDto)
    {
        try
        {
            if (documentTypeDto.DocumentTypeName == null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNotChanged, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }
            var documentType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByIdAsync(documentTypeDto.DocumentTypeId);
            if (documentType == null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            }
            if (documentType.IsDeleted)
                return ResponseUtil.Error(ResponseMessages.DivisionHasDeleted, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);
            
            var documentTypeWithName = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByNameAsync(documentTypeDto.DocumentTypeName);
            if (documentTypeWithName != null)
            {
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNameExisted, ResponseMessages.OperationFailed, HttpStatusCode.NotFound);
            }

            documentType.DocumentTypeName = documentTypeDto.DocumentTypeName;
            await _unitOfWork.DocumentTypeUOW.UpdateAsync(documentType);
            await _unitOfWork.SaveChangesAsync();
            var result = _mapper.Map<DocumentTypeDto>(documentType);
            return ResponseUtil.GetObject(result, ResponseMessages.UpdateSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetAllDocumentTypeAsync(string? documentTypeName, int page, int limit)
    {
        try
        {
            var documentTypes = await _unitOfWork.DocumentTypeUOW.FindAllDocumentTypeAsync();
            
            if (!string.IsNullOrWhiteSpace(documentTypeName))
            {
                documentTypes = documentTypes
                    .Where(d => d.DocumentTypeName.ToLower().Contains(documentTypeName.ToLower()))
                    .ToList();
            }
            
            var totalRecords = documentTypes.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / limit);

            IEnumerable<DocumentType> documentTypeResults = documentTypes
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            var result = _mapper.Map<IEnumerable<DocumentTypeDto>>(documentTypeResults);
            return ResponseUtil.GetCollection(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, totalRecords, page, limit, totalPages);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> UpdateDocumentTypeActiveOrDeleteAsync(Guid documentTypeId)
    {
        try
        {
            if (documentTypeId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.DocumentTypeIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var documentType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByIdAsync(documentTypeId);
            if (documentType == null)
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);
            

            if (documentType.IsDeleted)
            {
                documentType.IsDeleted = false;
                await _unitOfWork.DocumentTypeUOW.UpdateAsync(documentType);
                await _unitOfWork.SaveChangesAsync();
                return ResponseUtil.GetObject(ResponseMessages.DivisionActive, ResponseMessages.UpdateSuccessfully,
                    HttpStatusCode.OK, 1);
            }

            documentType.IsDeleted = true;
            await _unitOfWork.DocumentTypeUOW.UpdateAsync(documentType);
            await _unitOfWork.SaveChangesAsync();
            return ResponseUtil.GetObject(ResponseMessages.DivisionHasDeleted, ResponseMessages.DeleteSuccessfully,
                HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetDocumentTypeDetails(Guid documentTypeId)
    {
        try
        {
            if (documentTypeId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.DocumentTypeIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var documentType = await _unitOfWork.DocumentTypeUOW.FindDocumentTypeByIdAsync(documentTypeId);
            if (documentType == null)
                return ResponseUtil.Error(ResponseMessages.DocumentTypeNotFound, ResponseMessages.OperationFailed,
                    HttpStatusCode.NotFound);

            var result = _mapper.Map<DocumentTypeDto>(documentType);
            return ResponseUtil.GetObject(result, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1);
        }
        catch (Exception ex)
        {
            return ResponseUtil.Error(ResponseMessages.FailedToSaveData, ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<ResponseDto> GetAllDocumentTypeNameByWorkflowIdAsync(Guid workflowId)
    {
        try
        {
            if (workflowId == Guid.Empty)
                return ResponseUtil.Error(ResponseMessages.WorkflowIdInvalid, ResponseMessages.OperationFailed,
                    HttpStatusCode.BadRequest);

            var documentTypeWorkflows = await _unitOfWork.DocumentTypeWorkflowUOW.FindAllDocumentTypeNameByWorkflowIdAsync(workflowId);
            var documentTypeName = new List<String>();
            foreach (var documentTypeWorkflow in documentTypeWorkflows)
            {
                    documentTypeName.Add(documentTypeWorkflow.DocumentType.DocumentTypeName);
            }


            return ResponseUtil.GetCollection(documentTypeName, ResponseMessages.GetSuccessfully, HttpStatusCode.OK, 1, 1, 1, 1);
        }
        catch (Exception e)
        {
            return ResponseUtil.Error(e.Message, ResponseMessages.OperationFailed, HttpStatusCode.InternalServerError);
        }
    }
    
}