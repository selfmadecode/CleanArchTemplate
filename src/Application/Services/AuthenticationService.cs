using Application.DTO;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Helper;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly IRefreshTokenService _tokenRepo;
    private readonly IMailService _mailService;
    private readonly IEncryptService _encrypt;


    public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration,
        ApplicationDbContext dbContext, IRefreshTokenService tokenRepo, IMailService mailService,
        RoleManager<ApplicationRole> roleManager, IEncryptService encrypt)
    {
        _userManager = userManager;
        _configuration = configuration;
        _dbContext = dbContext;
        _tokenRepo = tokenRepo;
        _mailService = mailService;
        _roleManager = roleManager;
        _encrypt = encrypt;
    }

    public async Task<BaseResponse<JwtResponseDTO>> LoginAsync(LoginDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.EmailAddress);
        if (user == null)
        {
            return new BaseResponse<JwtResponseDTO>(ResponseMessage.UserNotFound,
                [ResponseMessage.UserNotFound]);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return new BaseResponse<JwtResponseDTO>(ResponseMessage.EmailNotConfirmed, [ResponseMessage.EmailNotConfirmed]);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
        {
            return new BaseResponse<JwtResponseDTO>(
                ResponseMessage.InCorrectUserNameOrPassword,
                [ResponseMessage.InCorrectUserNameOrPassword]
            );
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var (token, expiration) = CreateJwtTokenAsync(user, userRoles);
        var refreshToken = BuildRefreshToken();

        var userRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            CreatedBy = user.Id,
            Token = refreshToken,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("JWT:RefreshTokenExpiration").Value))
        };

        await SaveRefreshTokenAsync(userRefreshToken);

        var data = new JwtResponseDTO()
        {
            Token = token,
            Expiration = expiration,
            Roles = userRoles,
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            EmailAddress = user.Email
        };
        return new BaseResponse<JwtResponseDTO>(data);
    }

    public async Task<BaseResponse<Guid>> RegisterAsync(RegisterDTO model, string defaultRole = PermissionProvider.USER)
    {
        var normalizedEmail = model.EmailAddress.Trim().ToLowerInvariant();
        var normalizedPhone = model.PhoneNumber?.Trim();

        var user = new ApplicationUser
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail,
            PhoneNumber = normalizedPhone,
            EmailConfirmed = false,
            CreationTime = DateTime.UtcNow
        };

        var createUserResponse = await _userManager.CreateAsync(user, model.Password);
        if (!createUserResponse.Succeeded)
        {
            return new BaseResponse<Guid>(ResponseMessage.AccountCreationFailure, [..createUserResponse.Errors.Select(x => x.Description)]);
        };

        if (!await _roleManager.RoleExistsAsync(defaultRole))
            await _roleManager.CreateAsync(new ApplicationRole 
            {
                Name = defaultRole
            });

        var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);

        var code = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "EmailVerification");
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        if (defaultRole.Equals("user", StringComparison.OrdinalIgnoreCase))
            //await _mailService.SendAccountVerificationEmail(user.Email, user.FirstName.ToUpper(), EmailSubject.EmailConfirmation, emailConfirmationLink);
            await _mailService.SendAccountVerificationEmail(user.Email, $"{user.FirstName} {user.LastName}", EmailSubject.EmailConfirmation, code);
        else
            await _mailService.SendAdminAccountVerificationEmail(user.Email, user.FirstName, EmailSubject.EmailConfirmation,
                    code, model.Password);

        return new BaseResponse<Guid>(user.Id, ResponseMessage.AccountCreationSuccess);
    }

    public async Task<BaseResponse<JwtResponseDTO>> RefreshTokenAsync(string AccessToken, string RefreshToken)
    {
        ClaimsPrincipal? claimsPrincipal = GetPrincipalFromToken(AccessToken);
        if (claimsPrincipal == null)
            return ErrorResult(ResponseMessage.UserNotFound);

        var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return ErrorResult(ResponseMessage.UserNotFound);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ErrorResult(ResponseMessage.UserNotFound);

        var existingToken = await _tokenRepo.GetRefreshToken(user.Id, RefreshToken);
        if (existingToken == null)
            return ErrorResult(ResponseMessage.RefreshTokenNotFound);

        var userRoles = await _userManager.GetRolesAsync(user);

        var (jwtToken, expiration) = CreateJwtTokenAsync(user, userRoles);
        var newRefreshToken = BuildRefreshToken();

        _tokenRepo.DeleteRefreshTokenAsync(existingToken);
        await SaveNewRefreshTokenAsync(user.Id, newRefreshToken);

        var response = new JwtResponseDTO
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Token = jwtToken,
            Expiration = expiration,
            Roles = userRoles,
            RefreshToken = newRefreshToken
        };

        return new BaseResponse<JwtResponseDTO>(response);
    }

    public async Task<BaseResponse<Guid>> ChangePasswordAsync(Guid userId, ChangePasswordDTO model)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return new BaseResponse<Guid>(ResponseMessage.UserNotFound, []);
        }
        

        if (user.LockoutEnd < DateTime.UtcNow)
        {
            return new BaseResponse<Guid>(ResponseMessage.AccountLocked, []);
        }

        var changePassword = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!changePassword.Succeeded)
        {
            return new BaseResponse<Guid>(ResponseMessage.PasswordChangedFailure, [.. changePassword.Errors.Select(x =>x.Description)]);
        }        

        return new BaseResponse<Guid>(userId);
    }

    public async Task<BaseResponse<string>> ConfirmEmailAsync(ConfirmEmailDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return new BaseResponse<string>(ResponseMessage.UserNotFound, []);

        if (user.EmailConfirmed)
            return new BaseResponse<string>("Email already confirmed.", "Email already confirmed.");

        var isValid = await _userManager.VerifyUserTokenAsync(
            user,
            TokenOptions.DefaultEmailProvider,
            "EmailVerification",
            model.Code
        );

        if (!isValid)
            return new BaseResponse<string>("Invalid or expired verification code.");

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return new BaseResponse<string>("Email verified successfully.", "Email verified successfully.");
    }

    public async Task<BaseResponse<Guid>> ForgotPasswordAsync(ForgotPasswordDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.EmailAddress);

        if (user == null)
        {
            return new BaseResponse<Guid>(ResponseMessage.UserNotFound, []);
        }        

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var emailConfirmationLink = _mailService.GeneratePasswordResetLinkAsync(_encrypt.Encrypt(token), user.Email!);
        await _mailService.SendPasswordResetEmail(user.Email!, EmailSubject.PasswordReset, emailConfirmationLink, user.FirstName);

        return new BaseResponse<Guid>(user.Id);
    }

    public async Task<BaseResponse<Guid>> ResetPasswordAsync(ResetPasswordDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return new BaseResponse<Guid>(ResponseMessage.UserNotFound, []);
        }
        
        var token = _encrypt.Decrypt(model.Token);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, model.Password);

        if (!resetResult.Succeeded)
        {
            return new BaseResponse<Guid>(ResponseMessage.SomeThingWentWrong, [..resetResult.Errors.Select(x => x.Description)]);
        }
        

        return new BaseResponse<Guid>(user.Id);
    }

    public async Task<BaseResponse<UserDetailsDTO>> GetUserDetailsAndPermmissionsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return new BaseResponse<UserDetailsDTO>(ResponseMessage.UserNotFound, []);
        };

        var claims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var data = new UserDetailsDTO
        {
            EmailAddress = user.Email,
            Name = $"{user.FirstName} {user.LastName}",
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            UserId = user.Id,
            Roles = roles,
            Verified = user.EmailConfirmed,
            Permissions = claims.Select(x => x.Value).ToList()
        };

        return new BaseResponse<UserDetailsDTO>(data);
    }

    public async Task<BaseResponse<string>> LockoutUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return new BaseResponse<string>(ResponseMessage.UserNotFound, []);

        // Enable lockout if not already
        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;
            await _userManager.UpdateAsync(user);
        }

        // Set lockout end date to a far future date (e.g., 100 years)
        var lockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        if (result.Succeeded)
            return new BaseResponse<string>($"User '{userName}' locked out until {lockoutEnd:yyyy-MM-dd HH:mm:ss} UTC", []);

        return new BaseResponse<string>("Failed to lockout user", []);
    }

    public async Task<BaseResponse<string>> UnlockUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return new BaseResponse<string>(ResponseMessage.UserNotFound, []);

        // Remove lockout end date
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

        if (result.Succeeded)
        {
            user.AccessFailedCount = 0;
            await _userManager.UpdateAsync(user);
            return new BaseResponse<string>($"User '{userName}' has been unlocked");
        }

        return new BaseResponse<string>("Failed to unlock user", []);
    }

    private async Task SaveNewRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var expirationDays = Convert.ToInt32(_configuration["JWT:RefreshTokenExpiration"]);

        var newToken = new RefreshToken
        {
            CreatedBy = userId,
            UserId = userId,
            Token = refreshToken,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };

        await _tokenRepo.SaveRefreshTokenAsync(newToken);
    }

    private static BaseResponse<JwtResponseDTO> ErrorResult(string errorMessage)
    {
        var response = new BaseResponse<JwtResponseDTO>(errorMessage, [errorMessage]);
        return response;
    }

    private (string, DateTime) CreateJwtTokenAsync(ApplicationUser user, IList<string> userRoles)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var signingKey = new SymmetricSecurityKey(secretKey);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = BuildUserClaims(user, userRoles);

        var expiration = DateTime.UtcNow.AddMinutes(
            Convert.ToInt32(jwtSettings["DurationInMinutes"])
        );

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtSettings["ValidIssuer"],
            audience: jwtSettings["ValidAudience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiration,
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return (token, tokenDescriptor.ValidTo);
    }

    private static List<Claim> BuildUserClaims(ApplicationUser user, IList<string> userRoles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    private static string BuildRefreshToken()
    {
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        var randomNumber = new byte[32];

        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task SaveRefreshTokenAsync(RefreshToken model)
    {
        await _dbContext.AddAsync(model);
        await _dbContext.SaveChangesAsync();
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = _configuration["JWT:Secret"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT secret is missing from configuration.");

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
