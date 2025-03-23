using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class VerificationOtp
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; }
    public string? Otp { get; set; }
    public DateTime ExpirationTime { get; set; }
    public bool IsTrue { get; set; }
    public bool IsDeleted { get; set; }
    public int AttemptCount { get; set; }
    
    private const int EXPIRATION_TIME_MINUTES = 3;
    
    public Guid UserId { get; set; }
    public virtual User? User { get; set; } 
    
    public VerificationOtp()
    {
        // Default constructor
    }
    
    public VerificationOtp(string otp, Guid userId)
    {
        this.Otp = otp;
        this.UserId = userId;
        this.IsTrue = false;
        this.IsDeleted = false;
        this.AttemptCount = 0;
        this.ExpirationTime = GetTokenExpirationTime();
    }
    
    private DateTime GetTokenExpirationTime()
    {
        DateTime now = DateTime.Now; 
        DateTime expirationTime = now.AddMinutes(EXPIRATION_TIME_MINUTES).ToUniversalTime();
        return expirationTime;
    }
}