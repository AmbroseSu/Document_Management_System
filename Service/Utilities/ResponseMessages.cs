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

    public const string PasswordNotMatchConfirm = "New password and confirm password do not match";

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

    //RoleResource
    public const string RoleWithResourceCannotBeNull = "RoleResourceRequests cannot be null or empty";
    public const string RoleResourceNotExists = "RoleResource not found";
}