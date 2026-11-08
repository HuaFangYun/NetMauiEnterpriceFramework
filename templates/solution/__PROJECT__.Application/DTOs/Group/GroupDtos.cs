namespace __PROJECT__.Application.DTOs.Group;

public class GroupListItemDto
{
    public string GroupId { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public int UserCount { get; set; }
    public bool IsActive { get; set; }
    public int? StatusApproval { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class GroupEditDto
{
    public string GroupId { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public bool IsActive { get; set; } = true;
}
