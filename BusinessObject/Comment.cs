﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject;

public class Comment
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid CommentId { get; set; }
    public string? CommentContent { get; set; }
    private DateTime _createDate;
    public DateTime CreateDate {  
        get => _createDate.ToLocalTime();  
        set => _createDate = value.ToUniversalTime();  
    }
    public bool IsDeleted { get; set; }
    
    public Guid UserId { get; set; }
    public Guid TaskId { get; set; }
    public User? User { get; set; }
    public Task? Task { get; set; }

    /*public Comment()
    {
        CommentId = Guid.NewGuid();
    }*/
}