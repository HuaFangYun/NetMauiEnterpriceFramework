using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly __PROJECT__DbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IParameterStagingService _staging;

    public RoleManagementService(__PROJECT__DbContext context, ICurrentUserService currentUser, IParameterStagingService staging)
    {
        _context = context;
        _currentUser = currentUser;
        _staging = staging;
    }

    public async Task<GeneralListResponseDto<RoleListItemDto>> GetListAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = from ug in _context.um_user_group
                    join u in _context.um_user_app on ug.user_id equals u.user_id into uJoin
                    from u in uJoin.DefaultIfEmpty()
                    join g in _context.um_group_app on ug.group_id equals g.group_id into gJoin
                    from g in gJoin.DefaultIfEmpty()
                    where ug.isDeleted != true
                    select new RoleListItemDto
                    {
                        UserId = ug.user_id,
                        FullName = u != null ? u.full_name : null,
                        GroupId = ug.group_id,
                        GroupName = g != null ? g.group_name : null,
                        CanAddEdit = ug.can_add_edit,
                        CanDelete = ug.can_delete,
                        CanReadOnly = ug.can_read_only,
                        IsActive = ug.isActive == true,
                    };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.UserId.Contains(search)
                || (x.FullName != null && x.FullName.Contains(search))
                || (x.GroupName != null && x.GroupName.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        };

        var totalCount = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new GeneralListResponseDto<RoleListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }

    public async Task<RoleEditDto?> GetByIdAsync(string userId)
    {
        var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
        return role == null ? null : new RoleEditDto
        {
            UserId = role.user_id,
            GroupId = role.group_id,
            CanAddEdit = role.can_add_edit,
            CanDelete = role.can_delete,
            CanReadOnly = role.can_read_only,
            IsActive = role.isActive == true,
        };
    }

    public async Task<BaseGeneralResponse> CreateAsync(RoleEditDto dto)
    {
        try
        {
            dto.UserId = dto.UserId.Trim();
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.GroupId))
            {
                return Fail("User ID and group are required.");
            }

            if (!await _context.um_user_app.AnyAsync(e => e.user_id == dto.UserId && e.isDeleted != true))
            {
                return Fail("User not found.");
            }

            if (!await _context.um_group_app.AnyAsync(e => e.group_id == dto.GroupId && e.isDeleted != true))
            {
                return Fail("Group not found.");
            }

            if (await _context.um_user_group.AnyAsync(e => e.user_id == dto.UserId && e.isDeleted != true))
            {
                return Fail("User already has a role.");
            }

            if (await _staging.HasPendingAsync("um_user_group", "user_id", dto.UserId))
            {
                return Fail("Role already has a pending request.");
            }

            await _staging.StageAsync("um_user_group", Const.ApprovalType.ADD, BuildPayload(dto));
            return Success("Role creation request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(string userId, RoleEditDto dto)
    {
        try
        {
            var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
            if (role == null)
            {
                return Fail("Role not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.GroupId)
                || !await _context.um_group_app.AnyAsync(e => e.group_id == dto.GroupId && e.isDeleted != true))
            {
                return Fail("Group not found.");
            }

            if (await _staging.HasPendingAsync("um_user_group", "user_id", userId))
            {
                return Fail("Role already has a pending request.");
            }

            dto.UserId = userId;
            await _staging.StageAsync("um_user_group", Const.ApprovalType.MODIFIED, BuildPayload(dto));
            return Success("Role update request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string userId)
    {
        try
        {
            var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
            if (role == null)
            {
                return Fail("Role not found.");
            }

            if (await _staging.HasPendingAsync("um_user_group", "user_id", userId))
            {
                return Fail("Role already has a pending request.");
            }

            await _staging.StageAsync("um_user_group", Const.ApprovalType.DELETE, new Dictionary<string, string?>
            {
                ["user_id"] = userId,
                ["group_id"] = role.group_id,
            });
            return Success("Role deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static Dictionary<string, string?> BuildPayload(RoleEditDto dto) => new(StringComparer.OrdinalIgnoreCase)
    {
        ["user_id"] = dto.UserId,
        ["group_id"] = dto.GroupId,
        ["can_add_edit"] = dto.CanAddEdit ? "true" : "false",
        ["can_delete"] = dto.CanDelete ? "true" : "false",
        ["can_read_only"] = dto.CanReadOnly ? "true" : "false",
        ["is_active"] = dto.IsActive ? "true" : "false",
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
