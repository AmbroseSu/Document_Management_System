namespace DataAccess.DTO.Request;

public class SignRequest
{
    public string OtpCode { get; set; }
    public int Llx { get; set; }
    public int Lly { get; set; }
    public int Urx { get; set; }
    public int Ury { get; set; }
    public Guid DocumentId { get; set; }
}