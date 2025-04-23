namespace DataAccess.DTO.Request;

public class SignOptions
{
    public int PageNo { get; set; }
    public string Coordinate { get; set; }
    public bool? VisibleSignature { get; set; }
    public bool? VisualStatus { get; set; }
    public bool? ShowSignerInfo { get; set; }
    public string? SignerInfoPrefix { get; set; }
    public bool? ShowReason { get; set; }
    public string? SignReasonPrefix { get; set; }
    public string? SignReason { get; set; }
    public bool? ShowDateTime { get; set; }
    public string? DateTimePrefix { get; set; }
    public bool? ShowLocation { get; set; }
    public string? LocationPrefix { get; set; }
    public string? Location { get; set; }
    public string? TextDirection { get; set; }
    public string? TextColor { get; set; }
    public bool? ImageAndText { get; set; }
    public string? BackgroundImage { get; set; }
    public string? SignatureImage { get; set; }
}