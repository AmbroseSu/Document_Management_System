namespace DataAccess.DTO;

public class DocumentTypeDto
{
    public Guid DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public bool? IsDeleted { get; set; }

    //public List<Guid>? WorkflowIds { get; set; }
    //public List<Guid>? DocumentIds { get; set; }
    //public List<Guid>? ArchivedDocumentIds { get; set; }
}