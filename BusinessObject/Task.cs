﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace BusinessObject;

public class Task
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    private DateTime _startDate;
    public DateTime StartDate {  
        get => _startDate.ToLocalTime();  
        set => _startDate = value.ToUniversalTime();  
    }
    private DateTime _endDate;
    public DateTime EndDate {  
        get => _endDate.ToLocalTime();  
        set => _endDate = value.ToUniversalTime();  
    }
    public TaskStatus TaskStatus { get; set; }
    public TaskType TaskType { get; set; }
    private DateTime _createdDate;
    public DateTime CreatedDate {  
        get => _createdDate.ToLocalTime();  
        set => _createdDate = value.ToUniversalTime();  
    }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    
    public Guid StepId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public Step? Step { get; set; }
    public Document? Document { get; set; }
    public User? User { get; set; }
    public List<Comment>? Comments { get; set; }

    /*public Task()
    {
        TaskId = Guid.NewGuid();
    }*/
}