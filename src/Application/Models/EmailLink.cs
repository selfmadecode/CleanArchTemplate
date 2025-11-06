namespace Application.Models;

public class EmailLink
{
    public string? BaseUrl { get; set; }
    public string? ChangeDefaultPasswordUrl { get; set; }
    public string? ResetPasswordUrl { get; set; }
    public string? AdminChangePasswordUrl { get; set; }
    public string? AdminEmail { get; set; }
}
