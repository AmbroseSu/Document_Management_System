using BusinessObject;
using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class DocumentResponse
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? NumberOfDocument { get; set; }
    public string? SystemNumberOfDocument { get; set; }
    public string? DocumentContent { get; set; }
    public string? Sender { get; set; }
    public DateTime? DateReceived { get; set; }
    public string? DocumentTypeName { get; set; }
    public string? WorkflowName { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? DateIssued { get; set; }
    public DateTime? DateExpires { get; set; }
    public string? Scope { get; set; }
    public List<SignatureResponse>? DigitalSignatures { get; set; } = [];
    public List<SignatureResponse>? ApprovalSignatures { get; set; } = [];

    public List<VersionDetailRespone>? Versions { get; set; } = [];  
    public List<TasksResponse>? Tasks { get; set; } = [];
}

public class ArchiveDocumentResponse : DocumentResponse
{
    public bool CanGrant { get; set; }
    public bool CanDownLoad { get; set; }
    public List<Viewer> Viewers { get; set; } = [];
    public List<Viewer> Granters { get; set; } = [];
}

public class Viewer
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string Avatar { get; set; }
    public Guid UserId { get; set; }
    public string? DivisionName { get; set; }
}

public class SignatureResponse
{
    public string? SignerName { get; set; }
    public string? ImgUrl { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool IsDigital {get;set;}
}

public class VersionDetailRespone
{
    public string? VersionNumber { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? Url { get; set; }
    public bool? IsFinal { get; set; }
    public List<SizeDocumentResponse>? Sizes { get; set; }
}
public class SizeDocumentResponse
{
    public float width { get; set; }
    public float height { get; set; }
    public int page { get; set; }
}
public class TasksResponse
{
    public Guid TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string? Description { get; set; }
    public string? TaskType { get; set; }
    public string? Status { get; set; }
}