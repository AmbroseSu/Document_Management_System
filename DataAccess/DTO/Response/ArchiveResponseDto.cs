using System.Text.Json.Serialization;

namespace DataAccess.DTO.Response;

public class ArchiveResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("signBy")]
    public string SignBy { get; set; }

    [JsonPropertyName("createBy")]
    public string CreateBy { get; set; }
    

    [JsonPropertyName("numberOfDocument")]
    public string NumberOfDocument { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("createdBy")]
    public Guid CreatedBy { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("externalPartner")]
    public string ExternalPartner { get; set; }

    [JsonPropertyName("dateReceived")]
    public DateTime? DateReceived { get; set; }

    [JsonPropertyName("dateSented")]
    public DateTime? DateSented { get; set; }

    private sealed class CreateDateEqualityComparer : IEqualityComparer<ArchiveResponseDto>
    {
        public bool Equals(ArchiveResponseDto? x, ArchiveResponseDto? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.CreateDate.Equals(y.CreateDate);
        }

        public int GetHashCode(ArchiveResponseDto obj)
        {
            return obj.CreateDate.GetHashCode();
        }
    }

    public static IEqualityComparer<ArchiveResponseDto> CreateDateComparer { get; } = new CreateDateEqualityComparer();
}