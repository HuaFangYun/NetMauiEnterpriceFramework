using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Infrastructure.Services;

public class MenuManagementService : IMenuManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMenuQueryService _queryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IParameterStagingService _staging;
    private readonly IErrorLogService _errorLog;

    public MenuManagementService(
        IUnitOfWork unitOfWork,
        IMenuQueryService queryService,
        ICurrentUserService currentUser,
        IParameterStagingService staging,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _currentUser = currentUser;
        _staging = staging;
        _errorLog = errorLog;
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("MenuManagement", "ERROR", exception);

    public Task<GeneralListResponseDto<MenuListItemDto>> GetListAsync(DataTableRequestDto request)
        => _queryService.GetListAsync(request);

    public async Task<MenuEditDto?> GetByIdAsync(long id)
    {
        var menu = await _unitOfWork.Menus.GetActiveByIdAsync(id);
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

            if (await _unitOfWork.Menus.ExistsActiveByUniqueAsync(dto.MenuUnique))
            {
                return Fail("Menu unique already exists.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MenuUniqueParent)
                && !await _unitOfWork.Menus.ExistsActiveByUniqueAsync(dto.MenuUniqueParent))
            {
                return Fail("Parent menu not found.");
            }

            if (await _staging.HasPendingAsync("um_menu", "menu_unique", dto.MenuUnique))
            {
                return Fail("Menu already has a pending request.");
            }

            await _staging.StageAsync("um_menu", Const.ApprovalType.ADD, BuildPayload(dto));
            return Success("Menu creation request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(long id, MenuEditDto dto)
    {
        try
        {
            var menu = await _unitOfWork.Menus.GetActiveByIdAsync(id);
            if (menu == null)
            {
                return Fail("Menu not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.MenuUnique) || string.IsNullOrWhiteSpace(dto.MenuName))
            {
                return Fail("Menu unique and name are required.");
            }

            if (await _unitOfWork.Menus.ExistsAnyByUniqueExceptIdAsync(dto.MenuUnique, id))
            {
                return Fail("Menu unique already exists.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MenuUniqueParent) && dto.MenuUniqueParent == menu.menu_unique)
            {
                return Fail("Menu cannot be its own parent.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MenuUniqueParent)
                && !await _unitOfWork.Menus.ExistsActiveByUniqueAsync(dto.MenuUniqueParent))
            {
                return Fail("Parent menu not found.");
            }

            if (await _staging.HasPendingAsync("um_menu", "id", id.ToString()))
            {
                return Fail("Menu already has a pending request.");
            }

            dto.Id = id;
            await _staging.StageAsync("um_menu", Const.ApprovalType.MODIFIED, BuildPayload(dto));
            return Success("Menu update request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(long id)
    {
        try
        {
            var menu = await _unitOfWork.Menus.GetActiveByIdAsync(id);
            if (menu == null)
            {
                return Fail("Menu not found.");
            }

            if (await _unitOfWork.Menus.HasActiveChildrenAsync(menu.menu_unique))
            {
                return Fail("Menu still has child menus.");
            }

            if (await _staging.HasPendingAsync("um_menu", "id", id.ToString()))
            {
                return Fail("Menu already has a pending request.");
            }

            await _staging.StageAsync("um_menu", Const.ApprovalType.DELETE, new Dictionary<string, string?>
            {
                ["id"] = menu.id.ToString(),
                ["menu_unique"] = menu.menu_unique,
            });
            return Success("Menu deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
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

    public Task<List<MenuTreeItemDto>> GetTreeAsync()
        => _queryService.GetTreeAsync();

    public Task<List<long>> GetAssignedMenuIdsAsync(string groupId)
        => _queryService.GetAssignedMenuIdsAsync(groupId);

    public async Task<BaseGeneralResponse> SaveMenuMapAsync(MenuMapDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.GroupId))
            {
                return Fail("Group is required.");
            }

            if (!await _unitOfWork.UmGroupApp.ExistsActiveAsync(dto.GroupId))
            {
                return Fail("Group not found.");
            }

            var allMenus = await _unitOfWork.Menus.GetActiveMenusAsync();

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

            foreach (var root in allMenus.Where(e => e.parent_level_id == Const.ParentLevel.FIRST_PARENT))
            {
                requestedIds.Add(root.id);
            }

            var existing = await _unitOfWork.GroupMenus.GetByGroupAsync(dto.GroupId);
            _unitOfWork.GroupMenus.RemoveRange(existing);

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            foreach (var menuId in requestedIds)
            {
                await _unitOfWork.GroupMenus.AddAsync(new um_group_menu
                {
                    group_id = dto.GroupId,
                    menu_id = menuId,
                    statusApproval = Const.StatusApproval.APPROVED,
                    isActive = true,
                    isDeleted = false,
                    isLocked = false,
                    isDefault = false,
                    createdBy = actor,
                    createdDate = now,
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return Success("Menu map saved successfully.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
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
        Code = Const.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };
}
