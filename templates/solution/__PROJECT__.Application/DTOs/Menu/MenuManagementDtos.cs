namespace __PROJECT__.Application.DTOs.Menu;

public class MenuListItemDto
{
    public long Id { get; set; }
    public string MenuUnique { get; set; } = string.Empty;
    public string? MenuName { get; set; }
    public string? MenuUrl { get; set; }
    public string? ParentLevelId { get; set; }
    public string? MenuUniqueParent { get; set; }
    public string? ParentName { get; set; }
    public string? MenuIcon { get; set; }
    public string? MenuIconName { get; set; }
    public int? OrderSeq { get; set; }
    public string? ComponentType { get; set; }
    public bool IsActive { get; set; }
    public int? StatusApproval { get; set; }
}

public class MenuEditDto
{
    public long Id { get; set; }
    public string MenuUnique { get; set; } = string.Empty;
    public string? MenuName { get; set; }
    public string? MenuUrl { get; set; }
    public string? ParentLevelId { get; set; }
    public string? MenuUniqueParent { get; set; }
    public string? MenuIcon { get; set; }
    public int? OrderSeq { get; set; }
    public string? ComponentType { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MenuTreeItemDto
{
    public long Id { get; set; }
    public string MenuUnique { get; set; } = string.Empty;
    public string? MenuName { get; set; }
    public string? MenuIcon { get; set; }
    public string? ParentLevelId { get; set; }
    public string? MenuUniqueParent { get; set; }
    public int? OrderSeq { get; set; }
    public List<MenuTreeItemDto> Children { get; set; } = new();
}

public class MenuMapDto
{
    public string GroupId { get; set; } = string.Empty;
    public List<long> MenuIds { get; set; } = new();
}
