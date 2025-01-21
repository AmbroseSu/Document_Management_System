﻿using BusinessObject.Enums;

namespace DataAccess.DTO;

public class UserDto
{
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
    public bool IsDeleted { get; set; }
    public bool IsEnable { get; set; }
    
    public Guid? DivisionId { get; set; }
    public int? VerificationOtpId { get; set; }
    public List<Guid>? TaskUserIds { get; set; }
    public List<Guid>? CommentIds { get; set; }
    public List<Guid>? UserDocumentIds { get; set; }
    public List<Guid>? DeadlineIds { get; set; }
    public List<Guid>? UserDocumentPermissionIds { get; set; }
    public List<Guid>? UserRoleIds { get; set; }
}