namespace Service.Response;

public static class ResponseMessages
{
    public const string CreatedSuccessfully = "Successfully created";
    public const string UpdateSuccessfully = "Successfully updated";
    public const string GetSuccessfully = "Successfully get";
    public const string DeleteSuccessfully = "Successfully deleted";
    public const string OperationFailed = "Operation failed";
    public const string ValueNull = "Please insert a valid value";
    public const string FailedToSaveData = "Unable to save data to database";

    //User
    public const string UserNameAlreadyExists = "Username already exists";
    public const string InvalidEmailFormat = "Please insert a valid email format";
    public const string EmailAlreadyExists = "Email already exists";
    public const string EmailNotExists = "Email not exists";
    public const string EmailFormatInvalid = "Invalid email format";
    public const string UserIdInvalid = "Invalid user id";
    public const string UserHasUpdatedInformation = "User has updated information";
    public const string NoChangesDetected = "No changes detected";
    public const string PhoneNumberFormatInvalid = "Invalid phone number format";
    public const string FailedToSendEmail = "Tạo tài khoản thành công nhưng gửi email thất bại";
    public const string PasswordNotExists = "Password not exists";
    public const string PasswordChangeSuccess = "Password changed successfully";
    public const string OldPasswordIncorrect = "Old password is incorrect";
    public const string UserIdNull = "UserId cannot be null";
    public const string EmailNotMatch = "Email does not match";
    public const string SendEmailSuccessfully = "Send email successfully";
    public const string PasswordNotMatchConfirm = "New password and confirm password do not match";
    public const string ImportSuccessfully = "Import successfully";
    public const string UserNotRoleWithStep = "User role are incorrect for this step.";

    //public const string UserDelete = "User is deleted";
    public const string UserNotFound = "User not found";
    public const string UserActive = "User is active";
    public const string UserHasDeleted = "User has been deleted";
    public const string SendOtpSuccess = "Send OTP Success";
    public const string OtpHasNotExpired = "OTP has not expired, please wait 3 minutes to resend OTP.";
    public const string OtpNotFound = "OTP not found";
    public const string UserHasNotOtp = "OTP does not belong to this user.";
    public const string OtpExpired = "OTP has expired. Please request a new one.";
    public const string OtpHasUsed = "OTP has already been used.";
    public const string OtpVerified = "OTP verified successfully.";
    public const string OtpNotVerified = "OTP has not verified.";
    public const string OtpNotmatch = "OTP has not matched.";

    public const string OtpLocked = "Too many failed attempts. Please request a new OTP.";

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
    public const string NoRoleData = "No role data found";

    //RoleResource
    public const string RoleWithResourceCannotBeNull = "RoleResourceRequests cannot be null or empty";
    public const string RoleResourceNotExists = "RoleResource not found";
    
    //Divison
    public const string DivisionNameNotNull = "Division name cannot be null or empty";
    public const string DivisionNotChanged = "Division has not changed anything.";
    public const string DivisionNotFound = "Division not found";
    public const string DivisionNameExisted = "Division name already exists";
    public const string DivisionHasDeleted = "Division has been deleted";
    public const string DivisionIdInvalid = "Invalid division id";
    public const string DivisionActive = "Division is active";
    //public const string DivisionHasDeleted = "Division has been deleted";
    
    //Document
    public const string DocumentIdNull = "Document id cannot be null";
    public const string DocumentNotFound = "Document not found";
    public const string DocumentRejected = "Document has been rejected";
    public const string InvalidAction = "Invalid action";
    public const string DocumentCompleted = "Document has been completed";
    
    //DocumentType
    public const string DocumentTypeNameNotNull = "Document Type name cannot be null or empty";
    public const string DocumentTypeNameExisted = "Document Type name already exists";
    public const string DocumentTypeNotChanged = "Document Type has not changed anything.";
    public const string DocumentTypeNotFound = "Document Type not found";
    //public const string DocumentTypeNameExisted = "Document Type name already exists";
    public const string DocumentTypeIdInvalid = "Invalid document type id";
    
    //Workflow
    public const string WorkflowNameNotNull = "Workflow name cannot be null or empty";
    public const string WorkflowNameExisted = "Workflow name already exists";
    public const string WorkflowNotChanged = "Workflow has not changed anything.";
    public const string WorkflowNotFound = "Workflow not found";
    //public const string WorkflowNameExisted = "Workflow name already exists";
    public const string WorkflowIdInvalid = "Invalid workflow id";
    public const string WorkflowInvalid = "Workflow data is invalid";
    public const string WorkflowActive = "Workflow is active";
    public const string WorkflowHasDeleted = "Workflow has been deleted";
    
    //WorkflowFlow
    public const string RoleEndCurrentFlowNotMatchRoleStartNextFlow = "RoleEnd of current flow does not match RoleStart of next flow.";
    public const string WorkflowFlowNotFound = "WorkflowFlow not found";
    
    //Tasks
    public const string TaskStartdayEndDayFailed = "Start and end times must be greater than current";
    public const string TaskEndDayFailed = "End date must be greater than start date";
    public const string TaskNotFound = "Task not found";
    public const string TaskAlreadyDeleted = "Task already deleted";
    public const string TaskNotExists = "The task does not exist or is not within your control.";
    public const string TaskHadAccepted = "Task has been accepted";
    public const string TaskHadNotAccepted = "You have not accepted to process this task.";
    public const string TaskHadRejected = "Task has been rejected";
    public const string TaskHadNotCompleted = "It's not your turn to approve yet.";
    public const string TaskIdInvalid = "Invalid task id";
    
    //Step
    public const string StepIdNull = "Step id cannot be null";
}