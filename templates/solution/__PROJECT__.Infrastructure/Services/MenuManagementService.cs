using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class MenuManagementService : IMenuManagementService
{
    private readonly __PROJECT__DbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IParameterStagingService _staging;

    public MenuManagementService(__PROJECT__DbContext context, ICurrentUserService currentUser, IParameterStagingService staging)
    {
        _context = context;
        _currentUser = currentUser;
        _staging = staging;
    }

    public async Task<GeneralListResponseDto<MenuListItemDto>> GetListAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = from m in _context.um_menu
                    join i in _context.enum_menu_icon on m.menu_icon equals i.menu_icon_code into iJoin
                    from i in iJoin.DefaultIfEmpty()
                    join p in _context.um_menu on m.menu_unique_parent equals p.menu_unique into pJoin
                    from p in pJoin.DefaultIfEmpty()
                    where m.isDeleted != true
                    select new MenuListItemDto
                    {
                        Id = m.id,
                        MenuUnique = m.menu_unique,
                        MenuName = m.menu_name,
                        MenuUrl = m.menu_url,
                        ParentLevelId = m.parent_level_id,
                        MenuUniqueParent = m.menu_unique_parent,
                        ParentName = p != null ? p.menu_name : null,
                        MenuIcon = m.menu_icon,
                        MenuIconName = i != null
                            ? (i.menu_icon_name_alternate ?? i.menu_icon_name ?? m.menu_icon)
                            : m.menu_icon,
                        OrderSeq = m.order_seq,
                        ComponentType = m.component_type,
                        IsActive = m.isActive == true,
                        StatusApproval = m.statusApproval,
                    };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.MenuUnique.Contains(search)
                || (x.MenuName != null && x.MenuName.Contains(search))
                || (x.MenuUrl != null && x.MenuUrl.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "menuname" => desc ? query.OrderByDescending(x => x.MenuName) : query.OrderBy(x => x.MenuName),
            "menuunique" => desc ? query.OrderByDescending(x => x.MenuUnique) : query.OrderBy(x => x.MenuUnique),
            "parentlevelid" => desc ? query.OrderByDescending(x => x.ParentLevelId) : query.OrderBy(x => x.ParentLevelId),
            "orderseq" => desc ? query.OrderByDescending(x => x.OrderSeq) : query.OrderBy(x => x.OrderSeq),
            _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
        };

        var totalCount = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new GeneralListResponseDto<MenuListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }

    public async Task<MenuEditDto?> GetByIdAsync(long id)
    {
        var menu = await _context.um_menu.FirstOrDefaultAsync(e => e.id == id && e.isDeleted != true);
        return menu == null ? null : MapEdit(menu);
    }

    public async Task<BaseGeneralResponse> CreateAsync(MenuEditDto dto)
    {
        try
        {
            dto.MenuUnique = dto.MenuUnique.Trim();
            if (string.IsNullOrWhiteSpace(dto.MenuUnique) || string.IsNullOrWhiteSpace(dto.MenuName))
            {
                return Fail("Menu unique and name are required.");
            }

            if (await _context.um_menu.AnyAsync(e => e.menu_unique == dto.MenuUnique && e.isDeleted != true))
            {
                return Fail("Menu unique already exists.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MenuUniqueParent)
                && !await _context.um_menu.AnyAsync(e => e.menu_unique == dto.MenuUniqueParent && e.isDeleted != true))
            {
                return Fail("Parent menu not found.");
            }

            if (await _staging.HasPendingAsync("um_menu", "menu_unique", dto.MenuUnique))
            {
                return Fail("Menu already has a pending request.");
            }

            await _staging.StageAsync("um_menu", BCIConst.ApprovalType.ADD, BuildPayload(dto));
            return Success("Menu creation request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(long id, MenuEditDto dto)
    {
        try
        {
            var menu = await _context.um_menu.FirstOrDefaultAsync(e => e.id == id && e.isDeleted != true);
            if (menu == null)
            {
                return Fail("Menu not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.MenuUnique) || string.IsNullOrWhiteSpace(dto.MenuName))
            {
                return Fail("Menu unique and name are required.");
            }

            if (await _context.um_menu.AnyAsync(e => e.menu_unique == dto.MenuUnique && e.id != id))
            {
                return Fail("Menu unique already exists.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MenuUniqueParent) && dto.MenuUniqueParent == menu.menu_unique)
            {
                return Fail("Menu cannot be its own parent.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MenuUniqueParent)
                && !await _context.um_menu.AnyAsync(e => e.menu_unique == dto.MenuUniqueParent && e.isDeleted != true))
            {
                return Fail("Parent menu not found.");
            }

            if (await _staging.HasPendingAsync("um_menu", "id", id.ToString()))
            {
                return Fail("Menu already has a pending request.");
            }

            dto.Id = id;
            await _staging.StageAsync("um_menu", BCIConst.ApprovalType.MODIFIED, BuildPayload(dto));
            return Success("Menu update request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(long id)
    {
        try
        {
            var menu = await _context.um_menu.FirstOrDefaultAsync(e => e.id == id && e.isDeleted != true);
            if (menu == null)
            {
                return Fail("Menu not found.");
            }

            if (await _context.um_menu.AnyAsync(e => e.menu_unique_parent == menu.menu_unique && e.isDeleted != true))
            {
                return Fail("Menu still has child menus.");
            }

            if (await _staging.HasPendingAsync("um_menu", "id", id.ToString()))
            {
                return Fail("Menu already has a pending request.");
            }

            await _staging.StageAsync("um_menu", BCIConst.ApprovalType.DELETE, new Dictionary<string, string?>
            {
                ["id"] = menu.id.ToString(),
                ["menu_unique"] = menu.menu_unique,
            });
            return Success("Menu deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static Dictionary<string, string?> BuildPayload(MenuEditDto dto) => new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = dto.Id.ToString(),
        ["menu_unique"] = dto.MenuUnique,
        ["menu_name"] = dto.MenuName,
        ["menu_url"] = dto.MenuUrl,
        ["parent_level_id"] = dto.ParentLevelId,
        ["menu_unique_parent"] = dto.MenuUniqueParent,
        ["menu_icon"] = dto.MenuIcon,
        ["order_seq"] = dto.OrderSeq?.ToString(),
        ["component_type"] = dto.ComponentType,
        ["is_active"] = dto.IsActive ? "true" : "false",
    };

    public async Task<List<MenuTreeItemDto>> GetTreeAsync()
    {
        var menus = await _context.um_menu
            .Where(e => e.isDeleted != true)
            .OrderBy(e => e.order_seq)
            .Select(e => new MenuTreeItemDto
            {
                Id = e.id,
                MenuUnique = e.menu_unique,
                MenuName = e.menu_name,
                ParentLevelId = e.parent_level_id,
                MenuUniqueParent = e.menu_unique_parent,
                OrderSeq = e.order_seq,
            })
            .ToListAsync();

        var lookup = menus.ToLookup(e => e.MenuUniqueParent ?? string.Empty);
        foreach (var menu in menus)
        {
            menu.Children = lookup[menu.MenuUnique].OrderBy(e => e.OrderSeq).ToList();
        }

        return menus
            .Where(e => e.ParentLevelId == BCIConst.ParentLevel.FIRST_PARENT && string.IsNullOrEmpty(e.MenuUniqueParent))
            .OrderBy(e => e.OrderSeq)
            .ToList();
    }

    public Task<List<long>> GetAssignedMenuIdsAsync(string groupId)
        => _context.um_group_menu
            .Where(e => e.group_id == groupId && e.isActive != false && e.isDeleted != true)
            .Select(e => e.menu_id)
            .Where(e => e != null)
            .Select(e => e!.Value)
            .ToListAsync();

    public async Task<BaseGeneralResponse> SaveMenuMapAsync(MenuMapDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.GroupId))
            {
                return Fail("Group is required.");
            }

            if (!await _context.um_group_app.AnyAsync(e => e.group_id == dto.GroupId && e.isDeleted != true))
            {
                return Fail("Group not found.");
            }

            var allMenus = await _context.um_menu
                .Where(e => e.isDeleted != true)
                .Select(e => new { e.id, e.menu_unique, e.menu_unique_parent, e.parent_level_id })
                .ToListAsync();

            var byId = allMenus.ToDictionary(e => e.id);
            var byUnique = allMenus.ToDictionary(e => e.menu_unique, e => e);

            var requestedIds = dto.MenuIds.Where(id => byId.ContainsKey(id)).ToHashSet();

            foreach (var id in requestedIds.ToList())
            {
                var current = byId[id];
                var visited = new HashSet<long>();
                while (!string.IsNullOrWhiteSpace(current.menu_unique_parent)
                       && byUnique.TryGetValue(current.menu_unique_parent, out var parent)
                       && visited.Add(current.id))
                {
                    requestedIds.Add(parent.id);
                    current = parent;
                }
            }

            foreach (var root in allMenus.Where(e => e.parent_level_id == BCIConst.ParentLevel.FIRST_PARENT))
            {
                requestedIds.Add(root.id);
            }

            var existing = await _context.um_group_menu.Where(e => e.group_id == dto.GroupId).ToListAsync();
            _context.um_group_menu.RemoveRange(existing);

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            foreach (var menuId in requestedIds)
            {
                await _context.um_group_menu.AddAsync(new um_group_menu
                {
                    group_id = dto.GroupId,
                    menu_id = menuId,
                    statusApproval = BCIConst.StatusApproval.APPROVED,
                    isActive = true,
                    isDeleted = false,
                    isLocked = false,
                    isDefault = false,
                    createdBy = actor,
                    createdDate = now,
                });
            }

            await _context.SaveChangesAsync();
            return Success("Menu map saved successfully.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static MenuEditDto MapEdit(um_menu menu) => new()
    {
        Id = menu.id,
        MenuUnique = menu.menu_unique,
        MenuName = menu.menu_name,
        MenuUrl = menu.menu_url,
        ParentLevelId = menu.parent_level_id,
        MenuUniqueParent = menu.menu_unique_parent,
        MenuIcon = menu.menu_icon,
        OrderSeq = menu.order_seq,
        ComponentType = menu.component_type,
        IsActive = menu.isActive == true,
    };

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = BCIConst.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = BCIConst.ERROR_CODE,
        Message = message,
    };
}
