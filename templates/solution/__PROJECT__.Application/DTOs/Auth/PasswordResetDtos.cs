namespace __PROJECT__.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    public string UserIdOrEmail { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class PasswordResetRequestResult
{
    public Guid? OutboxId { get; set; }
    public bool DeliveryConfigured { get; set; }
    public bool UserFound { get; set; }
}
