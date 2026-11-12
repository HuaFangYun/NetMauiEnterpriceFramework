using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.Approval;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Queries;

public class ApprovalQueryService : IApprovalQueryService
{
    private readonly __PROJECT__DbContext _context;

    public ApprovalQueryService(__PROJECT__DbContext context) => _context = context;

    public async Task<GeneralListResponseDto<UserApprovalListItemDto>> GetPendingUsersAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
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
            "userid" => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "approvaltype" => desc ? query.OrderByDescending(x => x.ApprovalType) : query.OrderBy(x => x.ApprovalType),
            "requesterfullname" => desc ? query.OrderByDescending(x => x.RequesterFullName) : query.OrderBy(x => x.RequesterFullName),
            "createddate" => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        };

        var total = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        data.ForEach(x => x.ApprovalType = TypeLabel(x.ApprovalType));

        return new GeneralListResponseDto<UserApprovalListItemDto> { Data = data, TotalCount = total };
    }

    public async Task<GeneralListResponseDto<GroupApprovalListItemDto>> GetPendingGroupsAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
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
            "groupid" => desc ? query.OrderByDescending(x => x.GroupId) : query.OrderBy(x => x.GroupId),
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "approvaltype" => desc ? query.OrderByDescending(x => x.ApprovalType) : query.OrderBy(x => x.ApprovalType),
            "requesterfullname" => desc ? query.OrderByDescending(x => x.RequesterFullName) : query.OrderBy(x => x.RequesterFullName),
            "createddate" => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => desc ? query.OrderByDescending(x => x.GroupId) : query.OrderBy(x => x.GroupId),
        };

        var total = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        data.ForEach(x => x.ApprovalType = TypeLabel(x.ApprovalType));

        return new GeneralListResponseDto<GroupApprovalListItemDto> { Data = data, TotalCount = total };
    }

    public async Task<GeneralListResponseDto<RoleApprovalListItemDto>> GetPendingRolesAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var rows = await _context.tb_parameter_sytem_temporary
            .Where(e => e.table_name == "um_user_group"
                        && (e.type == Const.ApprovalType.ADD
                            || e.type == Const.ApprovalType.MODIFIED
                            || e.type == Const.ApprovalType.DELETE))
            .OrderByDescending(e => e.created_date)
            .ToListAsync(cancellationToken);

        var details = await LoadDetailsAsync(rows.Select(e => e.id), cancellationToken);
        var userIds = details.Values.Select(GetUserId).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        userIds.AddRange(rows.Select(e => e.created_by ?? string.Empty).Where(e => e.Length > 0));
        userIds = userIds.Distinct().ToList();
        var groupIds = details.Values.Select(values => Get(values, "group_id")).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();

        var users = await _context.um_user_app
            .Where(e => userIds.Contains(e.user_id))
            .ToDictionaryAsync(e => e.user_id, e => e.full_name, cancellationToken);
        var groups = await _context.um_group_app
            .Where(e => groupIds.Contains(e.group_id))
            .ToDictionaryAsync(e => e.group_id, e => e.group_name, cancellationToken);

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
            "userid" => desc ? list.OrderByDescending(x => x.UserId).ToList() : list.OrderBy(x => x.UserId).ToList(),
            "fullname" => desc ? list.OrderByDescending(x => x.FullName).ToList() : list.OrderBy(x => x.FullName).ToList(),
            "groupname" => desc ? list.OrderByDescending(x => x.GroupName).ToList() : list.OrderBy(x => x.GroupName).ToList(),
            "canadded" => desc ? list.OrderByDescending(x => x.CanAddEdit).ToList() : list.OrderBy(x => x.CanAddEdit).ToList(),
            "candelete" => desc ? list.OrderByDescending(x => x.CanDelete).ToList() : list.OrderBy(x => x.CanDelete).ToList(),
            "canreadonly" => desc ? list.OrderByDescending(x => x.CanReadOnly).ToList() : list.OrderBy(x => x.CanReadOnly).ToList(),
            "approvaltype" => desc ? list.OrderByDescending(x => x.ApprovalType).ToList() : list.OrderBy(x => x.ApprovalType).ToList(),
            "requesterfullname" => desc ? list.OrderByDescending(x => x.RequesterFullName).ToList() : list.OrderBy(x => x.RequesterFullName).ToList(),
            "createddate" => desc ? list.OrderByDescending(x => x.CreatedDate).ToList() : list.OrderBy(x => x.CreatedDate).ToList(),
            _ => desc ? list.OrderByDescending(x => x.UserId).ToList() : list.OrderBy(x => x.UserId).ToList(),
        };

        return new GeneralListResponseDto<RoleApprovalListItemDto>
        {
            Data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
        };
    }

    public async Task<GeneralListResponseDto<MenuApprovalListItemDto>> GetPendingMenusAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var rows = await _context.tb_parameter_sytem_temporary
            .Where(e => e.table_name == "um_menu"
                        && (e.type == Const.ApprovalType.ADD
                            || e.type == Const.ApprovalType.MODIFIED
                            || e.type == Const.ApprovalType.DELETE))
            .OrderByDescending(e => e.created_date)
            .ToListAsync(cancellationToken);

        var details = await LoadDetailsAsync(rows.Select(e => e.id), cancellationToken);
        var requesterIds = rows.Select(e => e.created_by).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        var requesters = await _context.um_user_app
            .Where(e => requesterIds.Contains(e.user_id))
            .ToDictionaryAsync(e => e.user_id, e => e.full_name, cancellationToken);

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
            "menuunique" => desc ? list.OrderByDescending(x => x.MenuUnique).ToList() : list.OrderBy(x => x.MenuUnique).ToList(),
            "menuname" => desc ? list.OrderByDescending(x => x.MenuName).ToList() : list.OrderBy(x => x.MenuName).ToList(),
            "menuurl" => desc ? list.OrderByDescending(x => x.MenuUrl).ToList() : list.OrderBy(x => x.MenuUrl).ToList(),
            "menuuniqueparent" => desc ? list.OrderByDescending(x => x.MenuUniqueParent).ToList() : list.OrderBy(x => x.MenuUniqueParent).ToList(),
            "approvaltype" => desc ? list.OrderByDescending(x => x.ApprovalType).ToList() : list.OrderBy(x => x.ApprovalType).ToList(),
            "requesterfullname" => desc ? list.OrderByDescending(x => x.RequesterFullName).ToList() : list.OrderBy(x => x.RequesterFullName).ToList(),
            "createddate" => desc ? list.OrderByDescending(x => x.CreatedDate).ToList() : list.OrderBy(x => x.CreatedDate).ToList(),
            _ => desc ? list.OrderByDescending(x => x.MenuUnique).ToList() : list.OrderBy(x => x.MenuUnique).ToList(),
        };

        return new GeneralListResponseDto<MenuApprovalListItemDto>
        {
            Data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
        };
    }

    private async Task<Dictionary<Guid, Dictionary<string, string?>>> LoadDetailsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        var rows = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => idList.Contains(e.tb_parameter_sytem_temporary_id))
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(e => e.tb_parameter_sytem_temporary_id)
            .ToDictionary(
                e => e.Key,
                e => e.ToDictionary(x => x.column_name, x => x.value, StringComparer.OrdinalIgnoreCase));
    }

    private static string? Get(Dictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) ? value : null;

    private static string GetUserId(Dictionary<string, string?> values) => Get(values, "user_id") ?? string.Empty;

    private static bool ParseBool(string? value)
        => value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static long ParseLong(string? value) => long.TryParse(value, out var result) ? result : 0;

    private static string TypeLabel(string? type) => type switch
    {
        Const.ApprovalType.ADD => "Add",
        Const.ApprovalType.MODIFIED => "Modify",
        Const.ApprovalType.DELETE => "Delete",
        _ => type ?? string.Empty,
    };
}
