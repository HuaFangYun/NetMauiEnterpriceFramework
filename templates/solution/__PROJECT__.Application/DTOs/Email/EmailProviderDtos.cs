namespace __PROJECT__.Application.DTOs.Email;

public class EmailProviderDto
{
    public int Id { get; set; }
    public string ProviderType { get; set; } = "SMTP";
    public string? ProviderName { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public bool HasPassword { get; set; }
    public string? Password { get; set; }

    public string? ClientId { get; set; }
    public bool HasClientSecret { get; set; }
    public bool HasRefreshToken { get; set; }
    public string? ClientSecret { get; set; }
    public string? RefreshToken { get; set; }

    public DateTime? LastTestedAt { get; set; }
    public string? LastTestStatus { get; set; }
    public string? LastTestMessage { get; set; }
}
