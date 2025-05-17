using System.Text.Json.Serialization;
using BusinessObject.Enums;
using Elastic.Clients.Elasticsearch.Serialization;

namespace BusinessObject.Option;

public class DocumentElastic
{
    [JsonPropertyName("documentid")]
    public Guid DocumentId { get; set; }

    [JsonPropertyName("documentname")]
    public string? DocumentName { get; set; }

    [JsonPropertyName("datereceived")]
    public DateTime? DateReceived { get; set; }

    [JsonPropertyName("numberofdocument")]
    public string? NumberOfDocument { get; set; }

    [JsonPropertyName("documentcontent")]
    public string? DocumentContent { get; set; }

    [JsonPropertyName("updateddate")]
    public DateTime? UpdatedDate { get; set; }

    [JsonPropertyName("processingstatus")]
    public int ProcessingStatus { get; set; }

    [JsonPropertyName("documentpriority")]
    public int DocumentPriority { get; set; }

    [JsonPropertyName("finalarchivedocumentid")]
    public Guid? FinalArchiveDocumentId { get; set; }

    [JsonPropertyName("@timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("documenttypeid")]
    public Guid? DocumentTypeId { get; set; }

    // [JsonPropertyName("expirationdate")]
    // public DateTime? ExpirationDate { get; set; }

    [JsonPropertyName("dateissued")]
    public DateTime? DateIssued { get; set; }

    [JsonPropertyName("userid")]
    public Guid? UserId { get; set; }

    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    [JsonPropertyName("deadline")]
    public DateTime? Deadline { get; set; }

    [JsonPropertyName("@version")]
    public string? Version { get; set; }

    [JsonPropertyName("isdeleted")]
    public bool IsDeleted { get; set; }

    [JsonPropertyName("createddate")]
    public DateTime? CreatedDate { get; set; }

    [JsonPropertyName("templatearchivedocumentid")]
    public Guid? TemplateArchiveDocumentId { get; set; }

    [JsonPropertyName("systemnumberofdoc")]
    public string? SystemNumberOfDoc { get; set; }

    [JsonPropertyName("signedby")]
    public string? SignedBy { get; set; }
    
}