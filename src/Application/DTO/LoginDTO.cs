using System.ComponentModel.DataAnnotations;

namespace Application.DTO;

public record LoginDTO
{
    [Required(ErrorMessage = "Email address is required")]
    public required string EmailAddress { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}

public record RegisterDTO
{
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [Required]
    [EmailAddress]
    public required string EmailAddress { get; set; }
    [Phone]
    public required string PhoneNumber { get; set; }
    [Required]
    public required string Password { get; set; }
}

public record RefreshTokenDTO
{
    [Required]
    public required string AccessToken { get; set; }
    [Required]
    public required string RefreshToken { get; set; }
}

public record ChangePasswordDTO
{
    [DataType(DataType.Password)]
    public required string OldPassword { get; set; }
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }
}

public record ConfirmEmailDTO
{
    [Required]
    public required string Code { get; set; }
    [Required]
    public required string Email { get; set; }
}

public record ForgotPasswordDTO
{
    [Required]
    [EmailAddress]
    public required string EmailAddress { get; set; }
}

public record ResetPasswordDTO
{
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
}

public record UserDetailsDTO
{
    public Guid UserId { get; set; }
    public required string EmailAddress { get; set; }
    public required string Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool Verified { get; set; }
    public IList<string> Permissions { get; set; } = [];
    public IList<string> Roles { get; set; } = [];
}
