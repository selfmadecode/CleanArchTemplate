namespace Application.Models;

public static class EmailTemplateUrl
{
    // windows OS
    //public const string Test = @"EmailTemplate\Test.html";
    //public const string AccountVerificationTemplate = @"EmailTemplate\Confirm_email.html";
    //public const string AdminAccountVerificationTemplate = @"EmailTemplate\Confirm_Email_Admin.html";
    //public const string PasswordResetTemplate = @"EmailTemplate\Reset_password.html";
    //public const string AccountConfirmationTemplate = @"EmailTemplate\AccountConfirmation.html";
    //public const string AccountBlockedTemplate = @"EmailTemplate\AccountBlocked.html";
    //public const string AccountUnBlockedTemplate = @"EmailTemplate\AccountUnBlocked.html";
    //public const string ContactUsTemplate = @"EmailTemplate\ContactUs.html";

    // Linux-based environment

    public const string Test = @"EmailTemplate/Test.html";
    public const string AccountVerificationTemplate = @"EmailTemplate/Confirm_email.html";
    public const string AdminAccountVerificationTemplate = @"EmailTemplate/Confirm_Email_Admin.html";
    public const string PasswordResetTemplate = @"EmailTemplate/Reset_password.html";
    public const string AccountConfirmationTemplate = @"EmailTemplate/AccountConfirmation.html";
    public const string AccountBlockedTemplate = @"EmailTemplate/AccountBlocked.html";
    public const string AccountUnBlockedTemplate = @"EmailTemplate/AccountUnBlocked.html";
    public const string ContactUsTemplate = @"EmailTemplate/ContactUs.html";

    // best case
    //public static readonly string Test = Path.Combine("EmailTemplate", "Test.html");
    //public static readonly string AccountVerificationTemplate = Path.Combine("EmailTemplate", "Confirm_email.html");
    //public static readonly string AdminAccountVerificationTemplate = Path.Combine("EmailTemplate", "Confirm_Email_Admin.html");
    //public static readonly string PasswordResetTemplate = Path.Combine("EmailTemplate", "Reset_password.html");
    //public static readonly string AccountConfirmationTemplate = Path.Combine("EmailTemplate", "AccountConfirmation.html");
    //public static readonly string AccountBlockedTemplate = Path.Combine("EmailTemplate", "AccountBlocked.html");
    //public static readonly string AccountUnBlockedTemplate = Path.Combine("EmailTemplate", "AccountUnBlocked.html");
    //public static readonly string ContactUsTemplate = Path.Combine("EmailTemplate", "ContactUs.html");

}
