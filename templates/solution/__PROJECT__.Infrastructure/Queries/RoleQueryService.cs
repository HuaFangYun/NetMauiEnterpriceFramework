using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Queries;

public class RoleQueryService : IRoleQueryService
{
    private readonly __PROJECT__DbContext _context;

    public RoleQueryService(__PROJECT__DbContext context) => _context = context;

    public async Task<GeneralListResponseDto<RoleListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
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

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GeneralListResponseDto<RoleListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }
}
