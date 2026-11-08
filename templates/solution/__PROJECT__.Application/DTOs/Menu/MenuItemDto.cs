namespace __PROJECT__.Application.DTOs.Menu;

public class MenuItemDto
{
    public long Id { get; set; }
    public string MenuUnique { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public string? MenuUrl { get; set; }
    public string? ParentLevelId { get; set; }
    public string? MenuUniqueParent { get; set; }
    public string? MenuIcon { get; set; }
    public int? OrderSeq { get; set; }
    public string? ComponentType { get; set; }
    public List<MenuItemDto> Children { get; set; } = new();
}
