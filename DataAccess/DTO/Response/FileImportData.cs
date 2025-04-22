using BusinessObject.Enums;

namespace DataAccess.DTO.Response;

public class FileImportData
{
    public String? FullName { get; set; }
    public String? UserName { get; set; }
    public String? Email { get; set; }
    public String? PhoneNumber { get; set; }
    public String? IdentityCard { get; set; }
    public String? Address { get; set; }
    public Gender Gender { get; set; }
    public String? Position { get; set; }
    public String? RoleName { get; set; }
    
}