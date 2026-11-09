using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Group;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class GroupManagementService : IGroupManagementService
{
    private readonly __PROJECT__DbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GroupManagementService(__PROJECT__DbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<GeneralListResponseDto<GroupListItemDto>> GetListAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = _context.um_group_app
            .Where(g => g.isDeleted != true)
            .Select(g => new GroupListItemDto
            {
                GroupId = g.group_id,
                GroupName = g.group_name,
                UserCount = _context.um_user_group.Count(ug => ug.group_id == g.group_id && ug.isDeleted != true),
                IsActive = g.isActive == true,
                StatusApproval = g.statusApproval,
                CreatedDate = g.createdDate,
            });

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.GroupId.Contains(search)
                || (x.GroupName != null && x.GroupName.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "usercount" => desc ? query.OrderByDescending(x => x.UserCount) : query.OrderBy(x => x.UserCount),
            "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => desc ? query.OrderByDescending(x => x.GroupId) : query.OrderBy(x => x.GroupId),
        };

        var totalCount = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new GeneralListResponseDto<GroupListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }

    public async Task<GroupEditDto?> GetByIdAsync(string groupId)
    {
        var group = await _context.um_group_app.FirstOrDefaultAsync(e => e.group_id == groupId && e.isDeleted != true);
        return group == null ? null : new GroupEditDto
        {
            GroupId = group.group_id,
            GroupName = group.group_name,
            IsActive = group.isActive == true,
        };
    }

    public async Task<BaseGeneralResponse> CreateAsync(GroupEditDto dto)
    {
        try
        {
            dto.GroupId = dto.GroupId.Trim();
            if (string.IsNullOrWhiteSpace(dto.GroupId))
            {
                return Fail("Group ID is required.");
            }

            if (await _context.um_group_app.AnyAsync(e => e.group_id == dto.GroupId))
            {
                return Fail("Group ID already exists.");
            }

            if (await _context.temp_group_app_pending.AnyAsync(e => e.group_id == dto.GroupId))
            {
                return Fail("Group already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _context.temp_group_app_pending.AddAsync(new temp_group_app_pending
            {
                group_id = dto.GroupId,
                group_name = dto.GroupName ?? string.Empty,
                approval_type_id = Const.ApprovalType.ADD,
                is_active = dto.IsActive,
                created_by = actor,
                created_date = now,
            });

            await _context.SaveChangesAsync();
            return Success("Group creation request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(string groupId, GroupEditDto dto)
    {
        try
        {
            var group = await _context.um_group_app.FirstOrDefaultAsync(e => e.group_id == groupId && e.isDeleted != true);
            if (group == null)
            {
                return Fail("Group not found.");
            }

            if (await _context.temp_group_app_pending.AnyAsync(e => e.group_id == groupId))
            {
                return Fail("Group already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _context.temp_group_app_pending.AddAsync(new temp_group_app_pending
            {
                group_id = groupId,
                group_name = dto.GroupName ?? string.Empty,
                approval_type_id = Const.ApprovalType.MODIFIED,
                is_active = dto.IsActive,
                created_by = actor,
                created_date = now,
            });

            await _context.SaveChangesAsync();
            return Success("Group update request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string groupId)
    {
        try
        {
            var group = await _context.um_group_app.FirstOrDefaultAsync(e => e.group_id == groupId && e.isDeleted != true);
            if (group == null)
            {
                return Fail("Group not found.");
            }

            if (groupId == Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE)
            {
                return Fail("Administrator group cannot be deleted.");
            }

            if (await _context.um_user_group.AnyAsync(e => e.group_id == groupId && e.isDeleted != true))
            {
                return Fail("Group is still assigned to one or more users.");
            }

            if (await _context.temp_group_app_pending.AnyAsync(e => e.group_id == groupId))
            {
                return Fail("Group already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _context.temp_group_app_pending.AddAsync(new temp_group_app_pending
            {
                group_id = groupId,
                group_name = group.group_name ?? string.Empty,
                approval_type_id = Const.ApprovalType.DELETE,
                is_active = false,
                created_by = actor,
                created_date = now,
            });

            await _context.SaveChangesAsync();
            return Success("Group deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

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
