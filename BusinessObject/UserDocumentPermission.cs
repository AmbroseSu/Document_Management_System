﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject;

public class UserDocumentPermission
{
    private DateTime _createdDate;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserDocumentPermissionId { get; set; }

    public DateTime CreatedDate
    {
        get => DateTime.SpecifyKind(_createdDate, DateTimeKind.Utc).ToLocalTime();
        set => _createdDate = value.ToUniversalTime();
    }
    
    public GrantPermission GrantPermission { get; set; }

    public bool IsDeleted { get; set; }


    public Guid UserId { get; set; }
    public Guid ArchivedDocumentId { get; set; }
    public User? User { get; set; }
    public ArchivedDocument? ArchivedDocument { get; set; }

    /*public UserDocumentPermission()
    {
        UserDocumentPermissionId = Guid.NewGuid();
    }*/
}