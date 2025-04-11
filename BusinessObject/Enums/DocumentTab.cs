namespace BusinessObject.Enums;

public enum DocumentTab
{
    All,
    Draft,
    PendingApproval,     // Đến lượt duyệt
    Waiting,             // Đang chờ duyệt
    Accepted,            // Đã chấp nhận
    Rejected,            // Đã từ chối
    Overdue 
}