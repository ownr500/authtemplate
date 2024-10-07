namespace API.Constants;

public static class MessageConstants
{
    public const string UserNotFound = "User not found";
    public const string UserAlreadyRegistered = "User already registered";
    public const string InvalidCredentials = "Invalid credentials";
    public const string CurrentPasswordNotMatch = "Current password doesn't match";
    public const string PasswordChangeFailed = "Password change faield";
    public const string InvalidRefreshToken = "Invalid refresh token";
    public const string InvalidRecoveryToken = "Invalid recovery token";
    public const string TokenExpired = "Token expired";
    public const string UserAlreadyHasRole = "User already has role";
    public const string UserHasNoRole = "User doesn't have role";
    public const string MailSubjectPasswordRecovery = "Password recovery";
}