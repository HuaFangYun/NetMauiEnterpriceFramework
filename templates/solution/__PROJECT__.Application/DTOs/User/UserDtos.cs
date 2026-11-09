namespace __PROJECT__.Application.DTOs.User;

public class UserListItemDto
{
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? GenderId { get; set; }
    public string? GenderName { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class UserEditDto
{
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? GenderId { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserActionDto
{
    public string UserId { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
}

public class UserFlagDto
{
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool? IsLogin { get; set; }
    public bool? IsFirstLogin { get; set; }
    public bool? IsLocked { get; set; }
    public int? FailedPasswordAttempt { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? Comment { get; set; }
}
