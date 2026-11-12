using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Queries;

public class UserQueryService : IUserQueryService
{
    private readonly __PROJECT__DbContext _context;

    public UserQueryService(__PROJECT__DbContext context) => _context = context;

    public async Task<GeneralListResponseDto<UserListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = from u in _context.um_user_app
                    join ug in _context.um_user_group on u.user_id equals ug.user_id into ugJoin
                    from ug in ugJoin.DefaultIfEmpty()
                    join g in _context.um_group_app on ug.group_id equals g.group_id into gJoin
                    from g in gJoin.DefaultIfEmpty()
                    join f in _context.um_user_app_flag on u.user_id equals f.user_id into fJoin
                    from f in fJoin.DefaultIfEmpty()
                    join e in _context.enum_gender on u.gender_id equals e.gender_code into eJoin
                    from e in eJoin.DefaultIfEmpty()
                    where u.isDeleted != true
                    select new UserListItemDto
                    {
                        UserId = u.user_id,
                        FullName = u.full_name,
                        Email = u.email,
                        PhoneNumber = u.phone_number,
                        GenderId = u.gender_id,
                        GenderName = e != null ? e.gender_name : null,
                        GroupId = ug != null ? ug.group_id : null,
                        GroupName = g != null ? g.group_name : null,
                        IsActive = u.isActive == true,
                        IsLocked = f != null && f.is_locked == true,
                        LastLoginDate = f != null ? f.last_login_date : null,
                    };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.UserId.Contains(search)
                || (x.FullName != null && x.FullName.Contains(search))
                || (x.Email != null && x.Email.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GeneralListResponseDto<UserListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }

    public async Task<GeneralListResponseDto<UserFlagDto>> GetUserFlagListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = from u in _context.um_user_app
                    join f in _context.um_user_app_flag on u.user_id equals f.user_id into flagJoin
                    from f in flagJoin.DefaultIfEmpty()
                    where u.isDeleted != true
                    select new UserFlagDto
                    {
                        UserId = u.user_id,
                        FullName = u.full_name,
                        IsLogin = f != null ? f.is_login : null,
                        IsFirstLogin = f != null ? f.is_first_login : null,
                        IsLocked = f != null ? f.is_locked : null,
                        FailedPasswordAttempt = f != null ? f.failed_password_attempt_count : null,
                        IpAddress = f != null ? f.ip_address : null,
                        LastLoginDate = f != null ? f.last_login_date : null,
                        Comment = f != null ? f.comment : null,
                    };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.UserId.Contains(search)
                || (x.FullName != null && x.FullName.Contains(search))
                || (x.IpAddress != null && x.IpAddress.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "islogin" => desc ? query.OrderByDescending(x => x.IsLogin) : query.OrderBy(x => x.IsLogin),
            "isfirstlogin" => desc ? query.OrderByDescending(x => x.IsFirstLogin) : query.OrderBy(x => x.IsFirstLogin),
            "islocked" => desc ? query.OrderByDescending(x => x.IsLocked) : query.OrderBy(x => x.IsLocked),
            "lastlogindate" => desc ? query.OrderByDescending(x => x.LastLoginDate) : query.OrderBy(x => x.LastLoginDate),
            _ => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GeneralListResponseDto<UserFlagDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }
}
