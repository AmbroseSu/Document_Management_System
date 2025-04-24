using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class User
{
    private DateTime _createdAt;
    private DateTime? _dateOfBirth;
    private DateTime _updatedAt;

    protected bool Equals(User other)
    {
        return UserId.Equals(other.UserId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((User)obj);
    }

    public override int GetHashCode()
    {
        return UserId.GetHashCode();
    }

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
    public string? IdentityCard { get; set; }

    public DateTime CreatedAt
    {
        get => DateTime.SpecifyKind(_createdAt, DateTimeKind.Utc).ToLocalTime();
        set => _createdAt = value.ToUniversalTime();
    }
    

    public DateTime UpdatedAt
    {
        get => DateTime.SpecifyKind(_updatedAt, DateTimeKind.Utc).ToLocalTime();
        set => _updatedAt = value.ToUniversalTime();
    }

    public string? FcmToken { get; set; }
    public string? Position { get; set; }

    public DateTime? DateOfBirth
    {
        get => _dateOfBirth.HasValue ? DateTime.SpecifyKind(_dateOfBirth.Value, DateTimeKind.Utc).ToLocalTime() : (DateTime?)null;
        set => _dateOfBirth = value?.ToUniversalTime();
    }

    public bool IsDeleted { get; set; }
    public bool IsEnable { get; set; }

    public Guid? DivisionId { get; set; }
    public Division? Division { get; set; }

    public List<VerificationOtp>? VerificationOtps { get; set; }
    public List<Comment>? Comments { get; set; }
    public List<UserDocumentPermission>? UserDocumentPermissions { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<Tasks>? Tasks { get; set; }
    public List<Document>? Documents { get; set; }
    public List<DigitalCertificate>? DigitalCertificates { get; set; }

    /*public User()
    {
        UserId = Guid.NewGuid();
    }*/
}