using Application.DTO;
using Application.Interfaces;
using Application.Models;
using Domain.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuthController : BaseController
{
    private readonly IAuthenticationService _authService;
    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }
    
    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<JwtResponseDTO>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var response = await _authService.LoginAsync(model);
        return ReturnResponse(response);
    }

    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<RegisterDTO>), 200)]
    public async Task<IActionResult> Register([FromBody] RegisterDTO model)
    {
        var response = await _authService.RegisterAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Route("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<JwtResponseDTO>), 200)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
    {
        var response = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Route("change-password")]
    [ProducesResponseType(typeof(BaseResponse<Guid>), 200)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
    {
        var response = await _authService.ChangePasswordAsync(UserId, model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Route("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO model)
    {
        var response = await _authService.ConfirmEmailAsync(model);
        return ReturnResponse(response);
    }

    [HttpPost]
    [Route("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<Guid>), 200)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
    {
        var response = await _authService.ForgotPasswordAsync(model);
        return ReturnResponse(response);
    }

    [HttpPost]
    [Route("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<Guid>), 200)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
    {
        var response = await _authService.ResetPasswordAsync(model);
        return ReturnResponse(response);
    }

    [HttpGet]
    [Route("profile")]
    [ProducesResponseType(typeof(BaseResponse<UserDetailsDTO>), 200)]
    public async Task<IActionResult> Profile()
    {
        var response = await _authService.GetUserDetailsAndPermmissionsAsync(UserId);
        return ReturnResponse(response);
    }

    [HttpGet]
    [Authorize]
    [Route("{userId}/profile")]
    [ProducesResponseType(typeof(BaseResponse<UserDetailsDTO>), 200)]
    public async Task<IActionResult> Profile(Guid userId)
    {
        var response = await _authService.GetUserDetailsAndPermmissionsAsync(userId);
        return ReturnResponse(response);
    }

    [HttpPost]
    [Authorize(Roles = PermissionProvider.SUPERADMIN)]
    [Route("{userName}/lockout")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    public async Task<IActionResult> LockoutUser(string userName)
    {
        var response = await _authService.LockoutUserAsync(userName);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Authorize(Roles = PermissionProvider.SUPERADMIN)]
    [Route("{userName}/unlock")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    public async Task<IActionResult> UnlockUser(string userName)
    {
        var response = await _authService.UnlockUserAsync(userName);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Route("admin-register")]
    [Authorize(Roles = PermissionProvider.SUPERADMIN)]
    [ProducesResponseType(typeof(BaseResponse<RegisterDTO>), 200)]
    public async Task<IActionResult> AdminRegister([FromBody] RegisterDTO model)
    {
        var response = await _authService.RegisterAsync(model, PermissionProvider.ADMIN);
        return StatusCode(response.StatusCode, response);
    }
}
