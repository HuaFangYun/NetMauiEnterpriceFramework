namespace __PROJECT__.Application.DTOs.Approval;

public class UserApprovalListItemDto
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string ApprovalType { get; set; } = string.Empty;
    public string? RequesterFullName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int SelectedAction { get; set; } = 2;
}

public class GroupApprovalListItemDto
{
    public long Id { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string ApprovalType { get; set; } = string.Empty;
    public string? RequesterFullName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int SelectedAction { get; set; } = 2;
}

public class RoleApprovalListItemDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
    public bool CanAddEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReadOnly { get; set; }
    public string ApprovalType { get; set; } = string.Empty;
    public string? RequesterFullName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int SelectedAction { get; set; } = 2;
}

public class MenuApprovalListItemDto
{
    public Guid Id { get; set; }
    public long MenuId { get; set; }
    public string MenuUnique { get; set; } = string.Empty;
    public string? MenuName { get; set; }
    public string? MenuUrl { get; set; }
    public string? ParentLevelId { get; set; }
    public string? MenuUniqueParent { get; set; }
    public string? MenuIcon { get; set; }
    public string? OrderSeq { get; set; }
    public string? ComponentType { get; set; }
    public string ApprovalType { get; set; } = string.Empty;
    public string? RequesterFullName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int SelectedAction { get; set; } = 2;
}
