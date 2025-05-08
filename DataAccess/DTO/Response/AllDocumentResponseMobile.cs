using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class AllDocumentResponseMobile
{
    public Guid? WorkFlowId { get; set; }
    public string? WorkFlowName { get; set; } 
    public List<DocumentTypeResponseMobile>? DocumentTypes { get; set; }
}
public class DocumentTypeResponseMobile
{
    public Guid DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; } 
    public float? Percent { get; set; } = 0;
    public List<DocumentResponseMobile>? DocumentResponseMobiles { get; set; }

    protected bool Equals(DocumentTypeResponseMobile other)
    {
        return DocumentTypeId.Equals(other.DocumentTypeId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DocumentTypeResponseMobile)obj);
    }

    public override int GetHashCode()
    {
        return DocumentTypeId.GetHashCode();
    }
}
public  class DocumentResponseMobile
{
    public Guid WorkFlowId { get; set; }

    public Guid Id { get; set; }
    public string? DocumentName { get; set; } 
    public DateTime? CreatedDate { get; set; }
    public string? Size { get; set; } = "0 Kb";
    public List<SizeDocumentResponse>? Sizes { get; set; }
}

public class DocumentDetailResponse
{
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; } 
    public string? DocumentContent { get; set; } 
    public string? NumberOfDocument { get; set; } 
    public ProcessingStatus ProcessingStatus { get; set; }
    public DateTime? DateExpired { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Receiver { get; set; } 
    public string? Sender { get; set; } 
    public List<Viewer> ViewerList { get; set; }
    public List<Viewer> GranterList { get; set; }
    public List<Viewer> ApproveByList { get; set; } = [];
    public string? WorkFlowName { get; set; } 
    public string? Scope { get; set; } 
    public string? SystemNumberDocument { get; set; } 
    public DateTime? DateIssued { get; set; }
    public string? DocumentTypeName { get; set; } 
    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; } 
    public List<string>? DivisionList { get; set; } 
    public List<Viewer>? UserList { get; set; }
    public List<string>? SignBys { get; set; }
    public string? DocumentUrl { get; set; } 
    public List<SizeDocumentResponse>? Sizes { get; set; }
}

public class UserResponseMobile
{
    public Guid? UserId { get; set; }
    public string? FullName { get; set; } 
    public string? DivisionName { get; set; } 
}