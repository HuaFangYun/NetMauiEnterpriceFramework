using __PROJECT__.Application.DTOs.Auth;
using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Application.Interfaces;

public interface IPasswordResetService
{
    Task<PasswordResetRequestResult> RequestAsync(string userIdOrEmail, string? ipAddress, string? publicBaseUrl = null);
    Task<bool> ValidateTokenAsync(string token);
    Task<BaseGeneralResponse> ResetAsync(string token, string newPassword, string confirmNewPassword);
}
