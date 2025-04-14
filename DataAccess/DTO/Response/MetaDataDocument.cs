namespace DataAccess.DTO.Response;

public class MetaDataDocument
{
    public string SignatureName { get; set; }
    public string? Issuer { get; set; }
    public string? SignerName { get; set; }
    public DateTime SingingDate { get; set; }
    public string Reason { get; set; }
    public string Location { get; set; }
    public bool IsValid {get; set; }
    public string? SerialNumber { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ExpirationDate { get; set; }
    
    public string Algorithm { get; set; }
}