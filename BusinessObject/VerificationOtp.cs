using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class VerificationOtp
{
    private const int EXPIRATION_TIME_MINUTES = 3;
    private DateTime _expirationTime;

    public VerificationOtp()
    {
        // Default constructor
    }

    public VerificationOtp(string otp, Guid userId)
    {
        Otp = otp;
        UserId = userId;
        IsTrue = false;
        IsDeleted = false;
        AttemptCount = 0;
        ExpirationTime = GetTokenExpirationTime();
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; }

    public string? Otp { get; set; }

    public DateTime ExpirationTime
    {
        get => DateTime.SpecifyKind(_expirationTime, DateTimeKind.Utc).ToLocalTime();
        set => _expirationTime = value.ToUniversalTime();
    }

    public bool IsTrue { get; set; }
    public bool IsDeleted { get; set; }
    public int AttemptCount { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    private DateTime GetTokenExpirationTime()
    {
        var now = DateTime.Now;
        var expirationTime = now.AddMinutes(EXPIRATION_TIME_MINUTES);
        return expirationTime;
    }
}