﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class Document
{
    private DateTime _createdDate;
    private DateTime _deadline;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid DocumentId { get; set; }

    public string? DocumentName { get; set; }
    public string? DocumentContent { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SignedBy { get; set; }
    public string? DocumentUrl { get; set; }

    public DateTime CreatedDate
    {
        get => _createdDate.ToLocalTime();
        set => _createdDate = value.ToUniversalTime();
    }

    public DateTime Deadline
    {
        get => _deadline.ToLocalTime();
        set => _deadline = value.ToUniversalTime();
    }

    public ProcessingStatus ProcessingStatus { get; set; }
    public DocumentPriority DocumentPriority { get; set; }
    public string? Sender { get; set; }
    public string? DateReceived { get; set; }
    public string? DateIssued { get; set; }
    public bool IsDeleted { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? TemplateArchiveDocumentId { get; set; }
    public ArchivedDocument? TemplateArchiveDocument { get; set; }
    public Guid? FinalArchiveDocumentId { get; set; }
    public ArchivedDocument? FinalArchiveDocument { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }

    public List<Task>? Tasks { get; set; }
    public List<DocumentSignature>? DocumentSignatures { get; set; }
    public List<AttachmentDocument>? AttachmentDocuments { get; set; }
    public List<DocumentVersion>? DocumentVersions { get; set; }
    public List<DocumentWorkflowStatus>? DocumentWorkflowStatuses { get; set; }


    /*public Document()
    {
        DocumentId = Guid.NewGuid();
    }*/
}