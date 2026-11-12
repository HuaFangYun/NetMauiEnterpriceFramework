using System.Reflection;

using MudBlazor;

namespace __PROJECT__.Shared.Utils;

public static class MenuIconResolver
{
    private static readonly Type FilledIcons = typeof(Icons.Material.Filled);

    public static string GetIcon(string? iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return Icons.Material.Filled.Circle;
        }

        var name = iconName switch
        {
            "fa fa-gear" => "Settings",
            "fa fa-user-circle" => "EmojiPeople",
            "fa fa-group" => "Groups",
            "fa fa-user-o" => "Hail",
            "fa fa-pagelines" => "MenuBook",
            "fa fa-cc-jcb" => "AdminPanelSettings",
            "fa fa-list-ul" => "FormatListBulleted",
            "fa fa-th-list" => "Checklist",
            "fa fa-cc-mastercard" => "AddModerator",
            "fa fa-cc-paypal" => "GppGood",
            "fa fa-flag" => "Flag",
            "fa fa-map-pin" => "PinDrop",
            "fa fa-flask" => "Science",
            "fa fa-envelope" => "Email",
            "GroupWork" => "Groups",
            "SettingsApplications" => "Settings",
            "SystemUpdateAlt" => "SystemUpdateAlt",
            "SettingsSystemDaydream" => "Settings",
            "FileCopy" => "FileCopy",
            _ => iconName,
        };

        var field = FilledIcons.GetField(name, BindingFlags.Public | BindingFlags.Static);
        if (field?.GetValue(null) is string icon)
        {
            return icon;
        }

        if (char.IsLower(name[0]))
        {
            var pascalCase = char.ToUpperInvariant(name[0]) + name[1..];
            field = FilledIcons.GetField(pascalCase, BindingFlags.Public | BindingFlags.Static);
            if (field?.GetValue(null) is string pascalIcon)
            {
                return pascalIcon;
            }
        }

        return Icons.Material.Filled.Circle;
    }
}
