namespace __PROJECT__.Application.DTOs.Role;

public class RoleListItemDto
{
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
    public bool CanAddEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReadOnly { get; set; }
    public string? ResponsibleToUserId { get; set; }
    public bool IsActive { get; set; }
}

public class RoleEditDto
{
    public string UserId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public bool CanAddEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReadOnly { get; set; }
    public string? ResponsibleToUserId { get; set; }
    public bool IsActive { get; set; } = true;
}
