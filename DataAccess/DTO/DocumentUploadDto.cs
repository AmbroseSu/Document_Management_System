namespace DataAccess.DTO;

public class DocumentUploadDto
{
    public Guid DocumentId { get; set; }
    public string? Name { get; set; }
    public string? Sender { get; set; }
    public DateTime? DateReceived { get; set; }
    public DateTime? ValidFrom { get; set; } // Signing date of the last signature
    public DateTime? ValidTo { get; set; } // Expiration date of the last signature's certificate
    public string? NumberOfDocument { get; set; }
    public string? DocumentTypeName { get; set; }
    public string? WorkflowName { get; set; } // Workflow name if available
    public List<string?> SignBy { get; set; } // List of signers
    public string? DocumentContent { get; set; }
}