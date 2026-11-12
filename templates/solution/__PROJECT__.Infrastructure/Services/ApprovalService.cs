using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.Approval;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class ApprovalService : IApprovalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalQueryService _queryService;
    private readonly AuditLogDbContext _audit;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IErrorLogService _errorLog;

    public ApprovalService(
        IUnitOfWork unitOfWork,
        IApprovalQueryService queryService,
        AuditLogDbContext audit,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _audit = audit;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _errorLog = errorLog;
    }

    private string Actor => _currentUser.UserId ?? "system";

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("Approval", "ERROR", exception);

    public Task<GeneralListResponseDto<UserApprovalListItemDto>> GetPendingUsersAsync(DataTableRequestDto request)
        => _queryService.GetPendingUsersAsync(request);

    public async Task<BaseGeneralResponse> ApproveUserAsync(long id)
    {
        var pending = await _unitOfWork.TempUserAppPending.GetByIdAsync(id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        if (pending.approval_type_id == Const.ApprovalType.ADD)
        {
            if (string.IsNullOrWhiteSpace(pending.password))
            {
                return Fail("Password payload is missing.");
            }

        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _unitOfWork.UmUserApp.GetByIdAsync(pending.user_id);
            var now = DateTime.Now;

            if (pending.approval_type_id == Const.ApprovalType.ADD)
            {
                if (user != null && user.isDeleted != true)
                {
                    return Fail("User already exists.");
                }

                if (user == null)
                {
                    user = new um_user_app
                    {
                        user_id = pending.user_id,
                        createdBy = pending.created_by,
                        createdDate = pending.created_date,
                    };
                    await _unitOfWork.UmUserApp.AddAsync(user);
                }

                ApplyUserFields(user, pending);
                user.password = pending.password;
                user.last_password_change_date = now;
                user.statusApproval = Const.StatusApproval.APPROVED;
                user.isActive = pending.is_active != false;
                user.isDeleted = false;
                user.deletedBy = null;
                user.deletedDate = null;
                user.approvedBy = Actor;
                user.approvedDate = now;

                await EnsureUserRoleAsync(pending, now, resetPermissions: true);
                await EnsureUserFlagAsync(pending.user_id, now, isNew: true, passwordChanged: true);
            }
            else if (pending.approval_type_id == Const.ApprovalType.MODIFIED)
            {
                if (user == null || user.isDeleted == true)
                {
                    return Fail("User not found.");
                }

                ApplyUserFields(user, pending);
                if (!string.IsNullOrWhiteSpace(pending.password))
                {
                    user.password = pending.password;
                    user.last_password_change_date = now;
                }

                user.statusApproval = Const.StatusApproval.APPROVED;
                user.isActive = pending.is_active != false;
                user.approvedBy = Actor;
                user.approvedDate = now;
                user.modifiedBy = pending.created_by;
                user.modifiedDate = pending.created_date;

                await EnsureUserRoleAsync(pending, now, resetPermissions: false);
                await EnsureUserFlagAsync(pending.user_id, now, isNew: false, passwordChanged: !string.IsNullOrWhiteSpace(pending.password));
            }
            else if (pending.approval_type_id == Const.ApprovalType.DELETE)
            {
                if (user == null || user.isDeleted == true)
                {
                    return Fail("User not found.");
                }

                user.isActive = false;
                user.isDeleted = true;
                user.deletedBy = pending.created_by;
                user.deletedDate = now;
                user.statusApproval = Const.StatusApproval.APPROVED;
                user.approvedBy = Actor;
                user.approvedDate = now;

                var role = await _unitOfWork.UmUserGroup.GetByIdAsync(pending.user_id);
                if (role != null)
                {
                    role.isActive = false;
                    role.isDeleted = true;
                    role.deletedBy = pending.created_by;
                    role.deletedDate = now;
                }

                var flag = await _unitOfWork.UmUserAppFlag.GetByUserIdAsync(pending.user_id);
                if (flag != null)
                {
                    flag.is_login = false;
                    flag.session_id = null;
                }
            }
            else
            {
                return Fail("Unknown approval type.");
            }

            _unitOfWork.TempUserAppPending.Remove(pending);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("UserApproval", "APPROVE", $"User {pending.user_id} approved ({TypeLabel(pending.approval_type_id)}).");
            await NotifyAsync(pending.created_by, $"User {pending.user_id} approved.");
            return Success("User approval applied.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectUserAsync(long id)
    {
        var pending = await _unitOfWork.TempUserAppPending.GetByIdAsync(id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        _unitOfWork.TempUserAppPending.Remove(pending);
        await _unitOfWork.SaveChangesAsync();
        await AuditAsync("UserApproval", "REJECT", $"User {pending.user_id} rejected ({TypeLabel(pending.approval_type_id)}).");
        await NotifyAsync(pending.created_by, $"User {pending.user_id} rejected.");
        return Success("User request rejected.");
    }

    public Task<GeneralListResponseDto<GroupApprovalListItemDto>> GetPendingGroupsAsync(DataTableRequestDto request)
        => _queryService.GetPendingGroupsAsync(request);

    public async Task<BaseGeneralResponse> ApproveGroupAsync(long id)
    {
        var pending = await _unitOfWork.TempGroupAppPending.GetByIdAsync(id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var group = await _unitOfWork.UmGroupApp.GetByIdAsync(pending.group_id);
            var now = DateTime.Now;

            if (pending.approval_type_id == Const.ApprovalType.ADD)
            {
                if (group != null && group.isDeleted != true)
                {
                    return Fail("Group already exists.");
                }

                if (group == null)
                {
                    group = new um_group_app
                    {
                        group_id = pending.group_id,
                        createdBy = pending.created_by,
                        createdDate = pending.created_date ?? now,
                    };
                    await _unitOfWork.UmGroupApp.AddAsync(group);
                }

                group.group_name = pending.group_name;
                group.statusApproval = Const.StatusApproval.APPROVED;
                group.isActive = pending.is_active != false;
                group.isDeleted = false;
                group.deletedBy = null;
                group.deletedDate = null;
                group.approvedBy = Actor;
                group.approvedDate = now;
            }
            else if (pending.approval_type_id == Const.ApprovalType.MODIFIED)
            {
                if (group == null || group.isDeleted == true)
                {
                    return Fail("Group not found.");
                }

                group.group_name = pending.group_name;
                group.isActive = pending.is_active != false;
                group.statusApproval = Const.StatusApproval.APPROVED;
                group.modifiedBy = pending.created_by;
                group.modifiedDate = pending.created_date;
                group.approvedBy = Actor;
                group.approvedDate = now;
            }
            else if (pending.approval_type_id == Const.ApprovalType.DELETE)
            {
                if (group == null || group.isDeleted == true)
                {
                    return Fail("Group not found.");
                }

                if (group.group_id == Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE)
                {
                    return Fail("Administrator group cannot be deleted.");
                }

                if (await _unitOfWork.UmGroupApp.HasActiveUsersAsync(group.group_id))
                {
                    return Fail("Group is still assigned to one or more users.");
                }

                group.isActive = false;
                group.isDeleted = true;
                group.deletedBy = pending.created_by;
                group.deletedDate = now;
                group.statusApproval = Const.StatusApproval.APPROVED;
                group.approvedBy = Actor;
                group.approvedDate = now;
            }
            else
            {
                return Fail("Unknown approval type.");
            }

            _unitOfWork.TempGroupAppPending.Remove(pending);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("GroupApproval", "APPROVE", $"Group {pending.group_id} approved ({TypeLabel(pending.approval_type_id)}).");
            await NotifyAsync(pending.created_by, $"Group {pending.group_id} approved.");
            return Success("Group approval applied.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectGroupAsync(long id)
    {
        var pending = await _unitOfWork.TempGroupAppPending.GetByIdAsync(id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        _unitOfWork.TempGroupAppPending.Remove(pending);
        await _unitOfWork.SaveChangesAsync();
        await AuditAsync("GroupApproval", "REJECT", $"Group {pending.group_id} rejected ({TypeLabel(pending.approval_type_id)}).");
        await NotifyAsync(pending.created_by, $"Group {pending.group_id} rejected.");
        return Success("Group request rejected.");
    }

    public Task<GeneralListResponseDto<RoleApprovalListItemDto>> GetPendingRolesAsync(DataTableRequestDto request)
        => _queryService.GetPendingRolesAsync(request);

    public async Task<BaseGeneralResponse> ApproveRoleAsync(Guid id)
    {
        var temporary = await _unitOfWork.ParameterStaging.GetByIdAsync(id);
        if (temporary == null || temporary.table_name != "um_user_group")
        {
            return Fail("Pending request not found.");
        }

        var values = await LoadDetailsAsync(id);
        var userId = Get(values, "user_id");
        var groupId = Get(values, "group_id");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Fail("User payload is missing.");
        }

        if (string.IsNullOrWhiteSpace(groupId)
            || !await _unitOfWork.UmGroupApp.ExistsActiveAsync(groupId))
        {
            return Fail("Group not found.");
        }

        if (!await _unitOfWork.UmUserApp.ExistsActiveAsync(userId))
        {
            return Fail("User not found.");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var role = await _unitOfWork.UmUserGroup.GetByIdAsync(userId);
            var now = DateTime.Now;

            if (temporary.type == Const.ApprovalType.DELETE)
            {
                if (role == null || role.isDeleted == true)
                {
                    return Fail("Role not found.");
                }

                role.isActive = false;
                role.isDeleted = true;
                role.deletedBy = temporary.created_by;
                role.deletedDate = now;
                role.statusApproval = Const.StatusApproval.APPROVED;
                role.approvedBy = Actor;
                role.approvedDate = now;
            }
            else if (temporary.type == Const.ApprovalType.ADD || temporary.type == Const.ApprovalType.MODIFIED)
            {
                if (temporary.type == Const.ApprovalType.ADD && role != null && role.isDeleted != true)
                {
                    return Fail("User already has a role.");
                }

                if (temporary.type == Const.ApprovalType.MODIFIED && (role == null || role.isDeleted == true))
                {
                    return Fail("Role not found.");
                }

                if (role == null)
                {
                    role = new um_user_group
                    {
                        user_id = userId,
                        createdBy = temporary.created_by,
                        createdDate = temporary.created_date,
                    };
                    await _unitOfWork.UmUserGroup.AddAsync(role);
                }

                role.group_id = groupId;
                role.can_add_edit = ParseBool(Get(values, "can_add_edit"));
                role.can_delete = ParseBool(Get(values, "can_delete"));
                role.can_read_only = ParseBool(Get(values, "can_read_only"));
                role.statusApproval = Const.StatusApproval.APPROVED;
                role.isActive = Get(values, "is_active") == null || ParseBool(Get(values, "is_active"));
                role.isDeleted = false;
                role.deletedBy = null;
                role.deletedDate = null;
                role.approvedBy = Actor;
                role.approvedDate = now;
                role.modifiedBy = temporary.created_by;
                role.modifiedDate = temporary.created_date;
            }
            else
            {
                return Fail("Unknown approval type.");
            }

            await RemoveTemporaryAsync(temporary);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("RoleApproval", "APPROVE", $"Role {userId} approved ({TypeLabel(temporary.type)}).");
            await NotifyAsync(temporary.created_by, $"Role {userId} approved.");
            return Success("Role approval applied.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectRoleAsync(Guid id)
    {
        return await RejectTemporaryAsync(id, "um_user_group", "RoleApproval");
    }

    public Task<GeneralListResponseDto<MenuApprovalListItemDto>> GetPendingMenusAsync(DataTableRequestDto request)
        => _queryService.GetPendingMenusAsync(request);

    public async Task<BaseGeneralResponse> ApproveMenuAsync(Guid id)
    {
        var temporary = await _unitOfWork.ParameterStaging.GetByIdAsync(id);
        if (temporary == null || temporary.table_name != "um_menu")
        {
            return Fail("Pending request not found.");
        }

        var values = await LoadDetailsAsync(id);
        var menuUnique = Get(values, "menu_unique");
        if (string.IsNullOrWhiteSpace(menuUnique))
        {
            return Fail("Menu unique is required.");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var menuId = ParseLong(Get(values, "id"));
            var menu = menuId > 0
                ? await _unitOfWork.Menus.GetByIdAsync(menuId)
                : await _unitOfWork.Menus.GetByUniqueAsync(menuUnique);
            var now = DateTime.Now;

            if (temporary.type == Const.ApprovalType.DELETE)
            {
                if (menu == null || menu.isDeleted == true)
                {
                    return Fail("Menu not found.");
                }

                if (await _unitOfWork.Menus.HasActiveChildrenAsync(menu.menu_unique))
                {
                    return Fail("Menu still has child menus.");
                }

                menu.isActive = false;
                menu.isDeleted = true;
                menu.deletedBy = temporary.created_by;
                menu.deletedDate = now;
                menu.statusApproval = Const.StatusApproval.APPROVED;
                menu.approvedBy = Actor;
                menu.approvedDate = now;

                var assignments = await _unitOfWork.GroupMenus.GetByMenuIdAsync(menu.id);
                foreach (var assignment in assignments)
                {
                    assignment.isActive = false;
                    assignment.isDeleted = true;
                    assignment.deletedBy = temporary.created_by;
                    assignment.deletedDate = now;
                }
            }
            else if (temporary.type == Const.ApprovalType.ADD || temporary.type == Const.ApprovalType.MODIFIED)
            {
                if (temporary.type == Const.ApprovalType.ADD && menu != null && menu.isDeleted != true)
                {
                    return Fail("Menu unique already exists.");
                }

                if (temporary.type == Const.ApprovalType.MODIFIED && (menu == null || menu.isDeleted == true))
                {
                    return Fail("Menu not found.");
                }

                if (menu == null
                    ? await _unitOfWork.Menus.AnyByUniqueAsync(menuUnique)
                    : await _unitOfWork.Menus.ExistsAnyByUniqueExceptIdAsync(menuUnique, menu.id))
                {
                    return Fail("Menu unique already exists.");
                }

                var parentUnique = Get(values, "menu_unique_parent");
                if (!string.IsNullOrWhiteSpace(parentUnique))
                {
                    if (menu != null && parentUnique == menu.menu_unique)
                    {
                        return Fail("Menu cannot be its own parent.");
                    }

                    if (!await _unitOfWork.Menus.ExistsActiveByUniqueAsync(parentUnique))
                    {
                        return Fail("Parent menu not found.");
                    }
                }

                if (menu == null)
                {
                    menu = new um_menu
                    {
                        menu_unique = menuUnique,
                        createdBy = temporary.created_by,
                        createdDate = temporary.created_date,
                    };
                    await _unitOfWork.Menus.AddAsync(menu);
                }
                else if (menu.menu_unique != menuUnique)
                {
                    var children = await _unitOfWork.Menus.GetByParentAsync(menu.menu_unique);
                    foreach (var child in children)
                    {
                        child.menu_unique_parent = menuUnique;
                        child.modifiedBy = temporary.created_by;
                        child.modifiedDate = now;
                    }
                }

                menu.menu_unique = menuUnique;
                menu.menu_name = Get(values, "menu_name");
                menu.menu_url = Get(values, "menu_url");
                menu.parent_level_id = Get(values, "parent_level_id");
                menu.menu_unique_parent = parentUnique;
                menu.menu_icon = Get(values, "menu_icon");
                menu.order_seq = ParseInt(Get(values, "order_seq"));
                menu.component_type = Get(values, "component_type");
                menu.statusApproval = Const.StatusApproval.APPROVED;
                menu.isActive = Get(values, "is_active") == null || ParseBool(Get(values, "is_active"));
                menu.isDeleted = false;
                menu.deletedBy = null;
                menu.deletedDate = null;
                menu.approvedBy = Actor;
                menu.approvedDate = now;
                menu.modifiedBy = temporary.created_by;
                menu.modifiedDate = temporary.created_date;
            }
            else
            {
                return Fail("Unknown approval type.");
            }

            await RemoveTemporaryAsync(temporary);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("MenuApproval", "APPROVE", $"Menu {menuUnique} approved ({TypeLabel(temporary.type)}).");
            await NotifyAsync(temporary.created_by, $"Menu {menuUnique} approved.");
            return Success("Menu approval applied.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectMenuAsync(Guid id)
    {
        return await RejectTemporaryAsync(id, "um_menu", "MenuApproval");
    }

    private async Task<BaseGeneralResponse> RejectTemporaryAsync(Guid id, string tableName, string auditEntity)
    {
        var temporary = await _unitOfWork.ParameterStaging.GetByIdAsync(id);
        if (temporary == null || temporary.table_name != tableName)
        {
            return Fail("Pending request not found.");
        }

        var details = await _unitOfWork.ParameterStaging.GetDetailsAsync(id);
        var key = details
            .FirstOrDefault(e => e.column_name == "user_id" || e.column_name == "menu_unique")
            ?.value;

        await RemoveTemporaryAsync(temporary);
        await _unitOfWork.SaveChangesAsync();
        await AuditAsync(auditEntity, "REJECT", $"{tableName} {key} rejected ({TypeLabel(temporary.type)}).");
        await NotifyAsync(temporary.created_by, $"{tableName} {key} rejected.");
        return Success("Request rejected.");
    }

    private async Task EnsureUserRoleAsync(temp_user_app_pending pending, DateTime now, bool resetPermissions)
    {
        if (string.IsNullOrWhiteSpace(pending.group_id))
        {
            return;
        }

        var role = await _unitOfWork.UmUserGroup.GetByIdAsync(pending.user_id);
        if (role == null)
        {
            role = new um_user_group
            {
                user_id = pending.user_id,
                createdBy = pending.created_by,
                createdDate = pending.created_date,
            };
            await _unitOfWork.UmUserGroup.AddAsync(role);
            resetPermissions = true;
        }

        role.group_id = pending.group_id;
        if (resetPermissions)
        {
            role.can_add_edit = true;
            role.can_delete = true;
            role.can_read_only = false;
        }

        role.statusApproval = Const.StatusApproval.APPROVED;
        role.isActive = true;
        role.isDeleted = false;
        role.deletedBy = null;
        role.deletedDate = null;
        role.approvedBy = Actor;
        role.approvedDate = now;
    }

    private async Task EnsureUserFlagAsync(string userId, DateTime now, bool isNew, bool passwordChanged)
    {
        var flag = await _unitOfWork.UmUserAppFlag.GetByUserIdAsync(userId);
        if (flag == null)
        {
            flag = new um_user_app_flag { user_id = userId };
            await _unitOfWork.UmUserAppFlag.AddAsync(flag);
            isNew = true;
        }

        flag.is_locked = false;
        flag.failed_password_attempt_count = 0;

        if (isNew)
        {
            flag.is_first_login = true;
            flag.is_login = false;
            flag.last_login_date = now;
        }

        if (passwordChanged)
        {
            flag.last_password_changed_date = now;
            flag.is_login = false;
            flag.session_id = null;
        }
    }

    private static void ApplyUserFields(um_user_app user, temp_user_app_pending pending)
    {
        user.full_name = pending.full_name;
        user.email = pending.email;
        user.phone_number = pending.phone_number;
        user.gender_id = pending.gender_id;
    }

    private async Task<Dictionary<string, string?>> LoadDetailsAsync(Guid id)
    {
        var rows = await _unitOfWork.ParameterStaging.GetDetailsAsync(id);
        return rows.ToDictionary(x => x.column_name, x => x.value, StringComparer.OrdinalIgnoreCase);
    }

    private async Task RemoveTemporaryAsync(tb_parameter_sytem_temporary temporary)
    {
        var details = await _unitOfWork.ParameterStaging.GetDetailsAsync(temporary.id);
        _unitOfWork.ParameterStaging.Remove(temporary, details);
    }

    private async Task NotifyAsync(string? userId, string message)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        try
        {
            await _notificationService.CreateAsync(
                userId,
                "Approval",
                message,
                null,
                NotificationTypeIds.Approval);
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
        }
    }

    private async Task AuditAsync(string entity, string action, string message)
    {        try
        {
            if (message.Length > 4000)
            {
                message = message[..4000];
            }

            _audit.tb_log.Add(new tb_log
            {
                entity = entity,
                log_type = action,
                log_msg = message,
                created_by = Actor,
                created_date = DateTime.Now,
            });
            await _audit.SaveChangesAsync();
        }
        catch
        {
            // Audit logging must not roll back an approved business transaction.
        }
    }

    private static string? Get(Dictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) ? value : null;

    private static string? Get(Dictionary<Guid, Dictionary<string, string?>> details, Guid id, string key)
        => details.TryGetValue(id, out var values) ? Get(values, key) : null;

    private static string GetUserId(Dictionary<string, string?> values) => Get(values, "user_id") ?? string.Empty;

    private static bool ParseBool(string? value)
        => value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static long ParseLong(string? value) => long.TryParse(value, out var result) ? result : 0;

    private static int? ParseInt(string? value) => int.TryParse(value, out var result) ? result : null;

    private static string TypeLabel(string? type) => type switch
    {
        Const.ApprovalType.ADD => "Add",
        Const.ApprovalType.MODIFIED => "Modify",
        Const.ApprovalType.DELETE => "Delete",
        _ => type ?? string.Empty,
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
