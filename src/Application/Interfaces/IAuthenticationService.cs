using Application.DTO;
using Application.Models;

namespace Application.Interfaces;

public interface IAuthenticationService
{
    Task<BaseResponse<JwtResponseDTO>> LoginAsync(LoginDTO model);
    Task<BaseResponse<Guid>> RegisterAsync(RegisterDTO model, string defaultRole = "user");
    Task<BaseResponse<JwtResponseDTO>> RefreshTokenAsync(string AccessToken, string RefreshToken);
    Task<BaseResponse<Guid>> ChangePasswordAsync(Guid userId, ChangePasswordDTO model);
    Task<BaseResponse<string>> ConfirmEmailAsync(ConfirmEmailDTO model);
    Task<BaseResponse<Guid>> ForgotPasswordAsync(ForgotPasswordDTO model);
    Task<BaseResponse<Guid>> ResetPasswordAsync(ResetPasswordDTO model);
    Task<BaseResponse<UserDetailsDTO>> GetUserDetailsAndPermmissionsAsync(Guid userId);
    Task<BaseResponse<string>> LockoutUserAsync(string userName);
    Task<BaseResponse<string>> UnlockUserAsync(string userName);
}
