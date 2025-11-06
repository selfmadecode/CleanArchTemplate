namespace Application.Models;

public record ResponseMessage
{
    public const string UserNotFound = "User not found!";
    public const string EmailNotConfirmed = "Email not confirmed, kindly confirm your account";
    public const string InCorrectUserNameOrPassword = "Incorrect username or password";
    public const string AccountCreationSuccess = "Account created successfully";
    public const string AccountCreationFailure = "Account creation failed";
    public const string RefreshTokenNotFound = "Refresh token not found!";
    public const string AccountLocked = "Account is locked, contact admin";
    public const string PasswordChangedFailure = "Failed to change password";
    public const string SomeThingWentWrong = "Something went wrong, please try again later";
}
