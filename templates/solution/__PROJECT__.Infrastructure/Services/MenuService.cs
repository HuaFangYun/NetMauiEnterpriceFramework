using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly __PROJECT__DbContext _context;

    public MenuService(__PROJECT__DbContext context) => _context = context;

    public async Task<List<MenuItemDto>> GetMenuTreeByGroupIdAsync(string groupId)
    {
        var menuIds = await _context.um_group_menu
            .Where(gm => gm.group_id == groupId && gm.isActive != false && gm.isDeleted != true)
            .Select(gm => gm.menu_id)
            .Where(id => id != null)
            .Select(id => id!.Value)
            .ToListAsync();

        if (menuIds.Count == 0)
        {
            return new List<MenuItemDto>();
        }

        var menus = await (from m in _context.um_menu
                           join i in _context.enum_menu_icon
                               on m.menu_icon equals i.menu_icon_code into icons
                           from i in icons.DefaultIfEmpty()
                           where menuIds.Contains(m.id) && m.isActive != false && m.isDeleted != true
                           select new MenuItemDto
                           {
                               Id = m.id,
                               MenuUnique = m.menu_unique,
                               MenuName = m.menu_name ?? string.Empty,
                               MenuUrl = m.menu_url,
                               ParentLevelId = m.parent_level_id,
                               MenuUniqueParent = m.menu_unique_parent,
                               MenuIcon = i != null
                                   ? (i.menu_icon_name_alternate ?? i.menu_icon_name ?? "Circle")
                                   : "Circle",
                               OrderSeq = m.order_seq,
                               ComponentType = m.component_type,
                           })
            .ToListAsync();

        var lookup = menus.ToLookup(m => m.MenuUniqueParent ?? string.Empty);
        foreach (var menu in menus)
        {
            menu.Children = lookup[menu.MenuUnique].OrderBy(c => c.OrderSeq).ToList();
        }

        return menus
            .Where(m => m.ParentLevelId == "A" && string.IsNullOrEmpty(m.MenuUniqueParent))
            .OrderBy(m => m.OrderSeq)
            .ToList();
    }
}
