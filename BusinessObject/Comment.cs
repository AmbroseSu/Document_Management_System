﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Comment
{
    private DateTime _createDate;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid CommentId { get; set; }

    public string? CommentContent { get; set; }

    public DateTime CreateDate
    {
        get => DateTime.SpecifyKind(_createDate, DateTimeKind.Utc).ToLocalTime();
        set => _createDate = value.ToUniversalTime();
    }

    public bool IsDeleted { get; set; }

    public Guid UserId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public User? User { get; set; }
    public DocumentVersion? DocumentVersion { get; set; }

    /*public Comment()
    {
        CommentId = Guid.NewGuid();
    }*/
}