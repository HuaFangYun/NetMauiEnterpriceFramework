using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Application.DTOs.Auth;
using __PROJECT__.Application.Exceptions;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IErrorLogService _errorLog;

    public AuthController(IAuthService authService, IErrorLogService errorLog)
    {
        _authService = authService;
        _errorLog = errorLog;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("first-login/change-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangeFirstLoginPassword([FromBody] FirstLoginPasswordDto request)
    {
        try
        {
            await _authService.ChangeFirstLoginPasswordAsync(request);
            return Ok(ApiResponseFactory.Success("Password changed successfully. Please sign in with your new password."));
        }
        catch (AuthenticationException ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return BadRequest(ApiResponseFactory.Error<string>(ex.Message));
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(userId, request?.RefreshToken ?? string.Empty);
        return Ok(ApiResponseFactory.Success("Logged out successfully"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId);
        return user == null
            ? NotFound(ApiResponseFactory.NotFound<UserResponseDto>("User not found."))
            : Ok(ApiResponseFactory.Success(user, "User profile retrieved successfully"));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var user = await _authService.UpdateProfileAsync(userId, request);
            return user == null
                ? NotFound(ApiResponseFactory.NotFound<UserResponseDto>("User not found."))
                : Ok(ApiResponseFactory.Success(user, "Profile updated successfully"));
        }
        catch (AuthenticationException ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return BadRequest(ApiResponseFactory.Error<UserResponseDto>(ex.Message));
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(ApiResponseFactory.Success("Password changed successfully"));
        }
        catch (AuthenticationException ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return BadRequest(ApiResponseFactory.Error<string>(ex.Message));
        }
    }

    [HttpGet("session-time")]
    [Authorize]
    public IActionResult GetSessionTimeRemaining()
    {
        var expiryClaim = User.FindFirst("exp")?.Value;
        if (expiryClaim != null && long.TryParse(expiryClaim, out var exp))
        {
            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            var remaining = expiryTime - DateTime.UtcNow;
            return Ok(new { remainingSeconds = Math.Max(0, (int)remaining.TotalSeconds) });
        }

        return Ok(new { remainingSeconds = 0 });
    }
}
