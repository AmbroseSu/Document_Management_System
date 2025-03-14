namespace Service.Response;

public static class ResponseMessages
{
    public const string CreatedSuccessfully = "Successfully created";
    public const string OperationFailed = "Operation failed";
    public const string ValueNull = "Please insert a valid value";
    public const string FailedToSaveData = "Unable to save data to database";
    
    //User
    public const string UserNameAlreadyExists = "Username already exists";
    public const string InvalidEmailFormat = "Please insert a valid email format";
    public const string EmailAlreadyExists = "Email already exists";
    //Permission
    public const string PermissionAlreadyExists = "Permission already exists";
    public const string PermissionNotExistsWithApi = "No suitable permissions were found for the API";
    public const string PermissionNotMatch = "Permission not match with action of API";

    //Resource
    public const string ResourceAlreadyExists = "Resource already exists";
    
    //Role
    public const string RoleAlreadyExists = "Role already exists";
    public const string RoleNotExists = "Role not exists";
    
    //RolePermission
    public const string RoleWithPermissionAlreadyExists = "Role With Permission already exists";
}