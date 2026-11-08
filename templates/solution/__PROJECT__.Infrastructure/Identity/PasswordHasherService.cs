using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Infrastructure.Identity;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<um_user_app> _hasher = new();

    public string HashPassword(string password)
        => _hasher.HashPassword(new um_user_app(), password);

    public bool ValidatePassword(string enteredPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(new um_user_app(), storedHash, enteredPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.Split(',').First().Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
