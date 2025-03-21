using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public Gender Gender { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? FcmToken { get; set; }
    public string? Position { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsEnable { get; set; }
    
    public Guid? DivisionId { get; set; }
    public Division? Division { get; set; }
    
    public List<VerificationOtp>? VerificationOtps { get; set; }
    public List<Comment>? Comments { get; set; }
    public List<UserDocumentPermission>? UserDocumentPermissions { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<Task>? Tasks { get; set; }
    public List<Document>? Documents { get; set; }
    public List<DigitalCertificate>? DigitalCertificates { get; set; }

    /*public User()
    {
        UserId = Guid.NewGuid();
    }*/
}