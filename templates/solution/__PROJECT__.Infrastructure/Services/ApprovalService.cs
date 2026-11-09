using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.Approval;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class ApprovalService : IApprovalService
{
    private readonly __PROJECT__DbContext _context;
    private readonly AuditLogDbContext _audit;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public ApprovalService(
        __PROJECT__DbContext context,
        AuditLogDbContext audit,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _context = context;
        _audit = audit;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    private string Actor => _currentUser.UserId ?? "system";

    public async Task<GeneralListResponseDto<UserApprovalListItemDto>> GetPendingUsersAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = from pending in _context.temp_user_app_pending
                    join user in _context.um_user_app on pending.created_by equals user.user_id into userJoin
                    from user in userJoin.DefaultIfEmpty()
                    join grp in _context.um_group_app on pending.group_id equals grp.group_id into groupJoin
                    from grp in groupJoin.DefaultIfEmpty()
                    select new UserApprovalListItemDto
                    {
                        Id = pending.id,
                        UserId = pending.user_id,
                        FullName = pending.full_name,
                        Email = pending.email,
                        GroupId = pending.group_id,
                        GroupName = grp != null ? grp.group_name : null,
                        ApprovalType = pending.approval_type_id ?? string.Empty,
                        RequesterFullName = user != null ? user.full_name : pending.created_by,
                        CreatedDate = pending.created_date,
                    };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.UserId.Contains(search)
                || (x.FullName != null && x.FullName.Contains(search))
                || (x.Email != null && x.Email.Contains(search))
                || (x.RequesterFullName != null && x.RequesterFullName.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "createddate" => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        };

        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        data.ForEach(x => x.ApprovalType = TypeLabel(x.ApprovalType));

        return new GeneralListResponseDto<UserApprovalListItemDto> { Data = data, TotalCount = total };
    }

    public async Task<BaseGeneralResponse> ApproveUserAsync(long id)
    {
        var pending = await _context.temp_user_app_pending.FirstOrDefaultAsync(e => e.id == id);
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

            if (string.IsNullOrWhiteSpace(pending.group_id)
                || !await _context.um_group_app.AnyAsync(e => e.group_id == pending.group_id && e.isDeleted != true))
            {
                return Fail("Group not found.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.um_user_app.FirstOrDefaultAsync(e => e.user_id == pending.user_id);
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
                    await _context.um_user_app.AddAsync(user);
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

                var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == pending.user_id);
                if (role != null)
                {
                    role.isActive = false;
                    role.isDeleted = true;
                    role.deletedBy = pending.created_by;
                    role.deletedDate = now;
                }

                var flag = await _context.um_user_app_flag.FirstOrDefaultAsync(e => e.user_id == pending.user_id);
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

            _context.temp_user_app_pending.Remove(pending);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("UserApproval", "APPROVE", $"User {pending.user_id} approved ({TypeLabel(pending.approval_type_id)}).");
            await NotifyAsync(pending.created_by, $"User {pending.user_id} approved.");
            return Success("User approval applied.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectUserAsync(long id)
    {
        var pending = await _context.temp_user_app_pending.FirstOrDefaultAsync(e => e.id == id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        _context.temp_user_app_pending.Remove(pending);
        await _context.SaveChangesAsync();
        await AuditAsync("UserApproval", "REJECT", $"User {pending.user_id} rejected ({TypeLabel(pending.approval_type_id)}).");
        await NotifyAsync(pending.created_by, $"User {pending.user_id} rejected.");
        return Success("User request rejected.");
    }

    public async Task<GeneralListResponseDto<GroupApprovalListItemDto>> GetPendingGroupsAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = from pending in _context.temp_group_app_pending
                    join user in _context.um_user_app on pending.created_by equals user.user_id into userJoin
                    from user in userJoin.DefaultIfEmpty()
                    select new GroupApprovalListItemDto
                    {
                        Id = pending.id,
                        GroupId = pending.group_id,
                        GroupName = pending.group_name,
                        ApprovalType = pending.approval_type_id ?? string.Empty,
                        RequesterFullName = user != null ? user.full_name : pending.created_by,
                        CreatedDate = pending.created_date,
                    };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.GroupId.Contains(search)
                || (x.GroupName != null && x.GroupName.Contains(search))
                || (x.RequesterFullName != null && x.RequesterFullName.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "createddate" => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => desc ? query.OrderByDescending(x => x.GroupId) : query.OrderBy(x => x.GroupId),
        };

        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        data.ForEach(x => x.ApprovalType = TypeLabel(x.ApprovalType));

        return new GeneralListResponseDto<GroupApprovalListItemDto> { Data = data, TotalCount = total };
    }

    public async Task<BaseGeneralResponse> ApproveGroupAsync(long id)
    {
        var pending = await _context.temp_group_app_pending.FirstOrDefaultAsync(e => e.id == id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var group = await _context.um_group_app.FirstOrDefaultAsync(e => e.group_id == pending.group_id);
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
                    await _context.um_group_app.AddAsync(group);
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

                if (await _context.um_user_group.AnyAsync(e => e.group_id == group.group_id && e.isDeleted != true))
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

            _context.temp_group_app_pending.Remove(pending);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("GroupApproval", "APPROVE", $"Group {pending.group_id} approved ({TypeLabel(pending.approval_type_id)}).");
            await NotifyAsync(pending.created_by, $"Group {pending.group_id} approved.");
            return Success("Group approval applied.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectGroupAsync(long id)
    {
        var pending = await _context.temp_group_app_pending.FirstOrDefaultAsync(e => e.id == id);
        if (pending == null)
        {
            return Fail("Pending request not found.");
        }

        _context.temp_group_app_pending.Remove(pending);
        await _context.SaveChangesAsync();
        await AuditAsync("GroupApproval", "REJECT", $"Group {pending.group_id} rejected ({TypeLabel(pending.approval_type_id)}).");
        await NotifyAsync(pending.created_by, $"Group {pending.group_id} rejected.");
        return Success("Group request rejected.");
    }

    public async Task<GeneralListResponseDto<RoleApprovalListItemDto>> GetPendingRolesAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var rows = await _context.tb_parameter_sytem_temporary
            .Where(e => e.table_name == "um_user_group"
                        && (e.type == Const.ApprovalType.ADD
                            || e.type == Const.ApprovalType.MODIFIED
                            || e.type == Const.ApprovalType.DELETE))
            .OrderByDescending(e => e.created_date)
            .ToListAsync();

        var details = await LoadDetailsAsync(rows.Select(e => e.id));
        var userIds = details.Values.Select(GetUserId).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        userIds.AddRange(rows.Select(e => e.created_by ?? string.Empty).Where(e => e.Length > 0));
        userIds = userIds.Distinct().ToList();
        var groupIds = details.Values.Select(values => Get(values, "group_id")).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();

        var users = await _context.um_user_app.Where(e => userIds.Contains(e.user_id)).ToDictionaryAsync(e => e.user_id, e => e.full_name);
        var groups = await _context.um_group_app.Where(e => groupIds.Contains(e.group_id)).ToDictionaryAsync(e => e.group_id, e => e.group_name);

        var list = rows.Select(row =>
        {
            var values = details.GetValueOrDefault(row.id) ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var userId = Get(values, "user_id") ?? string.Empty;
            var groupId = Get(values, "group_id");
            return new RoleApprovalListItemDto
            {
                Id = row.id,
                UserId = userId,
                FullName = users.GetValueOrDefault(userId),
                GroupId = groupId,
                GroupName = groupId == null ? null : groups.GetValueOrDefault(groupId),
                CanAddEdit = ParseBool(Get(values, "can_add_edit")),
                CanDelete = ParseBool(Get(values, "can_delete")),
                CanReadOnly = ParseBool(Get(values, "can_read_only")),
                ApprovalType = TypeLabel(row.type),
                RequesterFullName = users.GetValueOrDefault(row.created_by ?? string.Empty) ?? row.created_by,
                CreatedDate = row.created_date,
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            list = list.Where(x =>
                x.UserId.Contains(search)
                || (x.FullName != null && x.FullName.Contains(search))
                || (x.GroupName != null && x.GroupName.Contains(search))).ToList();
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        list = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? list.OrderByDescending(x => x.FullName).ToList() : list.OrderBy(x => x.FullName).ToList(),
            "groupname" => desc ? list.OrderByDescending(x => x.GroupName).ToList() : list.OrderBy(x => x.GroupName).ToList(),
            "createddate" => desc ? list.OrderByDescending(x => x.CreatedDate).ToList() : list.OrderBy(x => x.CreatedDate).ToList(),
            _ => desc ? list.OrderByDescending(x => x.UserId).ToList() : list.OrderBy(x => x.UserId).ToList(),
        };

        return new GeneralListResponseDto<RoleApprovalListItemDto>
        {
            Data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
        };
    }

    public async Task<BaseGeneralResponse> ApproveRoleAsync(Guid id)
    {
        var temporary = await _context.tb_parameter_sytem_temporary.FirstOrDefaultAsync(e => e.id == id && e.table_name == "um_user_group");
        if (temporary == null)
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
            || !await _context.um_group_app.AnyAsync(e => e.group_id == groupId && e.isDeleted != true))
        {
            return Fail("Group not found.");
        }

        if (!await _context.um_user_app.AnyAsync(e => e.user_id == userId && e.isDeleted != true))
        {
            return Fail("User not found.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId);
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
                    await _context.um_user_group.AddAsync(role);
                }

                role.group_id = groupId;
                role.can_add_edit = ParseBool(Get(values, "can_add_edit"));
                role.can_delete = ParseBool(Get(values, "can_delete"));
                role.can_read_only = ParseBool(Get(values, "can_read_only"));
                role.responsible_to_user_id = Get(values, "responsible_to_user_id");
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
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("RoleApproval", "APPROVE", $"Role {userId} approved ({TypeLabel(temporary.type)}).");
            await NotifyAsync(temporary.created_by, $"Role {userId} approved.");
            return Success("Role approval applied.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectRoleAsync(Guid id)
    {
        return await RejectTemporaryAsync(id, "um_user_group", "RoleApproval");
    }

    public async Task<GeneralListResponseDto<MenuApprovalListItemDto>> GetPendingMenusAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var rows = await _context.tb_parameter_sytem_temporary
            .Where(e => e.table_name == "um_menu"
                        && (e.type == Const.ApprovalType.ADD
                            || e.type == Const.ApprovalType.MODIFIED
                            || e.type == Const.ApprovalType.DELETE))
            .OrderByDescending(e => e.created_date)
            .ToListAsync();

        var details = await LoadDetailsAsync(rows.Select(e => e.id));
        var requesterIds = rows.Select(e => e.created_by).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        var requesters = await _context.um_user_app.Where(e => requesterIds.Contains(e.user_id)).ToDictionaryAsync(e => e.user_id, e => e.full_name);

        var list = rows.Select(row =>
        {
            var values = details.GetValueOrDefault(row.id) ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return new MenuApprovalListItemDto
            {
                Id = row.id,
                MenuId = ParseLong(Get(values, "id")),
                MenuUnique = Get(values, "menu_unique") ?? string.Empty,
                MenuName = Get(values, "menu_name"),
                MenuUrl = Get(values, "menu_url"),
                ParentLevelId = Get(values, "parent_level_id"),
                MenuUniqueParent = Get(values, "menu_unique_parent"),
                MenuIcon = Get(values, "menu_icon"),
                OrderSeq = Get(values, "order_seq"),
                ComponentType = Get(values, "component_type"),
                ApprovalType = TypeLabel(row.type),
                RequesterFullName = requesters.GetValueOrDefault(row.created_by ?? string.Empty) ?? row.created_by,
                CreatedDate = row.created_date,
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            list = list.Where(x =>
                x.MenuUnique.Contains(search)
                || (x.MenuName != null && x.MenuName.Contains(search))
                || (x.MenuUrl != null && x.MenuUrl.Contains(search))).ToList();
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        list = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "menuname" => desc ? list.OrderByDescending(x => x.MenuName).ToList() : list.OrderBy(x => x.MenuName).ToList(),
            "createddate" => desc ? list.OrderByDescending(x => x.CreatedDate).ToList() : list.OrderBy(x => x.CreatedDate).ToList(),
            _ => desc ? list.OrderByDescending(x => x.MenuUnique).ToList() : list.OrderBy(x => x.MenuUnique).ToList(),
        };

        return new GeneralListResponseDto<MenuApprovalListItemDto>
        {
            Data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
        };
    }

    public async Task<BaseGeneralResponse> ApproveMenuAsync(Guid id)
    {
        var temporary = await _context.tb_parameter_sytem_temporary.FirstOrDefaultAsync(e => e.id == id && e.table_name == "um_menu");
        if (temporary == null)
        {
            return Fail("Pending request not found.");
        }

        var values = await LoadDetailsAsync(id);
        var menuUnique = Get(values, "menu_unique");
        if (string.IsNullOrWhiteSpace(menuUnique))
        {
            return Fail("Menu unique is required.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var menuId = ParseLong(Get(values, "id"));
            var menu = menuId > 0
                ? await _context.um_menu.FirstOrDefaultAsync(e => e.id == menuId)
                : await _context.um_menu.FirstOrDefaultAsync(e => e.menu_unique == menuUnique);
            var now = DateTime.Now;

            if (temporary.type == Const.ApprovalType.DELETE)
            {
                if (menu == null || menu.isDeleted == true)
                {
                    return Fail("Menu not found.");
                }

                if (await _context.um_menu.AnyAsync(e => e.menu_unique_parent == menu.menu_unique && e.isDeleted != true))
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

                var assignments = await _context.um_group_menu.Where(e => e.menu_id == menu.id).ToListAsync();
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

                if (await _context.um_menu.AnyAsync(e => e.menu_unique == menuUnique && (menu == null || e.id != menu.id)))
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

                    if (!await _context.um_menu.AnyAsync(e => e.menu_unique == parentUnique && e.isDeleted != true))
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
                    await _context.um_menu.AddAsync(menu);
                }
                else if (menu.menu_unique != menuUnique)
                {
                    var children = await _context.um_menu.Where(e => e.menu_unique_parent == menu.menu_unique).ToListAsync();
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
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("MenuApproval", "APPROVE", $"Menu {menuUnique} approved ({TypeLabel(temporary.type)}).");
            await NotifyAsync(temporary.created_by, $"Menu {menuUnique} approved.");
            return Success("Menu approval applied.");
        }
        catch (Exception ex)
        {
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
        var temporary = await _context.tb_parameter_sytem_temporary.FirstOrDefaultAsync(e => e.id == id && e.table_name == tableName);
        if (temporary == null)
        {
            return Fail("Pending request not found.");
        }

        var key = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => e.tb_parameter_sytem_temporary_id == id && (e.column_name == "user_id" || e.column_name == "menu_unique"))
            .Select(e => e.value)
            .FirstOrDefaultAsync();

        await RemoveTemporaryAsync(temporary);
        await _context.SaveChangesAsync();
        await AuditAsync(auditEntity, "REJECT", $"{tableName} {key} rejected ({TypeLabel(temporary.type)}).");
        await NotifyAsync(temporary.created_by, $"{tableName} {key} rejected.");
        return Success("Request rejected.");
    }

    private async Task EnsureUserRoleAsync(temp_user_app_pending pending, DateTime now, bool resetPermissions)
    {
        var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == pending.user_id);
        if (role == null)
        {
            role = new um_user_group
            {
                user_id = pending.user_id,
                createdBy = pending.created_by,
                createdDate = pending.created_date,
            };
            await _context.um_user_group.AddAsync(role);
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
        var flag = await _context.um_user_app_flag.FirstOrDefaultAsync(e => e.user_id == userId);
        if (flag == null)
        {
            flag = new um_user_app_flag { user_id = userId };
            await _context.um_user_app_flag.AddAsync(flag);
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
        user.kode_cabang = pending.kode_cabang;
        user.gender_id = pending.gender_id;
        user.nomor_induk_karyawan = pending.nomor_induk_karyawan;
    }

    private async Task<Dictionary<Guid, Dictionary<string, string?>>> LoadDetailsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        var rows = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => idList.Contains(e.tb_parameter_sytem_temporary_id))
            .ToListAsync();

        return rows
            .GroupBy(e => e.tb_parameter_sytem_temporary_id)
            .ToDictionary(
                e => e.Key,
                e => e.ToDictionary(x => x.column_name, x => x.value, StringComparer.OrdinalIgnoreCase));
    }

    private async Task<Dictionary<string, string?>> LoadDetailsAsync(Guid id)
    {
        var all = await LoadDetailsAsync(new[] { id });
        return all.GetValueOrDefault(id) ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task RemoveTemporaryAsync(tb_parameter_sytem_temporary temporary)
    {
        var details = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => e.tb_parameter_sytem_temporary_id == temporary.id)
            .ToListAsync();
        _context.tb_parameter_sytem_temporary_detail.RemoveRange(details);
        _context.tb_parameter_sytem_temporary.Remove(temporary);
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
        catch
        {
            // Notification failure must not reverse an approved business transaction.
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
