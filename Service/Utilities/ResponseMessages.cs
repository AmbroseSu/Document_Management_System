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
    public const string EmailNotExists = "Email not exists";
    public const string EmailFormatInvalid = "Invalid email format";
    public const string PasswordNotExists = "Password not exists";
    public const string UserDelete = "User is deleted";
    //Permission
    public const string PermissionAlreadyExists = "Permission already exists";
    public const string PermissionNotExistsWithApi = "No suitable permissions were found for the API";
    public const string PermissionNotMatch = "Permission not match with action of API";

    //Resource
    public const string ResourceAlreadyExists = "Resource already exists";
    public const string ResourceCannotBeNull = "Resource cannot be null or empty";
    public const string ResourceNotExists = "Resource not found with ID";
    
    //Role
    public const string RoleAlreadyExists = "Role already exists";
    public const string RoleNotExists = "Role not found with ID";
    
    //RoleResource
    public const string RoleWithResourceCannotBeNull = "RoleResourceRequests cannot be null or empty";
    public const string RoleResourceNotExists = "RoleResource not found";
    
}