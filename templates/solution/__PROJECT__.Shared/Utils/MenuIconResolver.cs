using MudBlazor;

namespace __PROJECT__.Shared.Utils;

public static class MenuIconResolver
{
    public static string GetIcon(string? iconName) => iconName switch
    {
        "Settings" => Icons.Material.Filled.Settings,
        "EmojiPeople" => Icons.Material.Filled.EmojiPeople,
        "Groups" => Icons.Material.Filled.Groups,
        "Hail" => Icons.Material.Filled.Hail,
        "MenuBook" => Icons.Material.Filled.MenuBook,
        "AdminPanelSettings" => Icons.Material.Filled.AdminPanelSettings,
        "FormatListBulleted" => Icons.Material.Filled.FormatListBulleted,
        "Checklist" => Icons.Material.Filled.Checklist,
        "AddModerator" => Icons.Material.Filled.AddModerator,
        "GppGood" => Icons.Material.Filled.GppGood,
        "Flag" => Icons.Material.Filled.Flag,
        "PinDrop" => Icons.Material.Filled.PinDrop,
        _ => Icons.Material.Filled.Circle,
    };
}
