using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly __PROJECT__DbContext _context;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public UserManagementService(__PROJECT__DbContext context, IPasswordHasherService passwordHasher, ICurrentUserService currentUser)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<GeneralListResponseDto<UserListItemDto>> GetListAsync(DataTableRequestDto request)
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
                        KodeCabang = u.kode_cabang,
                        GenderId = u.gender_id,
                        GenderName = e != null ? e.gender_name : null,
                        NomorIndukKaryawan = u.nomor_induk_karyawan,
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
                || (x.Email != null && x.Email.Contains(search))
                || (x.NomorIndukKaryawan != null && x.NomorIndukKaryawan.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "kodecabang" => desc ? query.OrderByDescending(x => x.KodeCabang) : query.OrderBy(x => x.KodeCabang),
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        };

        var totalCount = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new GeneralListResponseDto<UserListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }

    public async Task<UserEditDto?> GetByIdAsync(string userId)
    {
        var user = await _context.um_user_app.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
        if (user == null)
        {
            return null;
        }

        var userGroup = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);

        return new UserEditDto
        {
            UserId = user.user_id,
            FullName = user.full_name,
            Email = user.email,
            PhoneNumber = user.phone_number,
            KodeCabang = user.kode_cabang,
            GenderId = user.gender_id,
            NomorIndukKaryawan = user.nomor_induk_karyawan,
            GroupId = userGroup?.group_id,
            IsActive = user.isActive == true,
        };
    }

    public async Task<BaseGeneralResponse> CreateAsync(UserEditDto dto)
    {
        try
        {
            dto.UserId = dto.UserId.Trim();
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return Fail("User ID and password are required.");
            }

            if (await _context.um_user_app.AnyAsync(e => e.user_id == dto.UserId))
            {
                return Fail("User ID already exists.");
            }

            if (string.IsNullOrWhiteSpace(dto.GroupId)
                || !await _context.um_group_app.AnyAsync(e => e.group_id == dto.GroupId && e.isDeleted != true))
            {
                return Fail("Group is invalid.");
            }

            if (await _context.temp_user_app_pending.AnyAsync(e => e.user_id == dto.UserId))
            {
                return Fail("User already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _context.temp_user_app_pending.AddAsync(new temp_user_app_pending
            {
                user_id = dto.UserId,
                full_name = dto.FullName,
                email = dto.Email,
                phone_number = dto.PhoneNumber,
                kode_cabang = dto.KodeCabang,
                gender_id = dto.GenderId,
                nomor_induk_karyawan = dto.NomorIndukKaryawan,
                group_id = dto.GroupId,
                password = _passwordHasher.HashPassword(dto.Password),
                approval_type_id = BCIConst.ApprovalType.ADD,
                is_active = dto.IsActive,
                is_deleted = false,
                created_by = actor,
                created_date = now,
            });

            await _context.SaveChangesAsync();
            return Success("User creation request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(string userId, UserEditDto dto)
    {
        try
        {
            var user = await _context.um_user_app.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
            if (user == null)
            {
                return Fail("User not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.GroupId)
                || !await _context.um_group_app.AnyAsync(e => e.group_id == dto.GroupId && e.isDeleted != true))
            {
                return Fail("Group is invalid.");
            }

            if (await _context.temp_user_app_pending.AnyAsync(e => e.user_id == userId))
            {
                return Fail("User already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _context.temp_user_app_pending.AddAsync(new temp_user_app_pending
            {
                user_id = userId,
                full_name = dto.FullName,
                email = dto.Email,
                phone_number = dto.PhoneNumber,
                kode_cabang = dto.KodeCabang,
                gender_id = dto.GenderId,
                nomor_induk_karyawan = dto.NomorIndukKaryawan,
                group_id = dto.GroupId,
                password = string.IsNullOrWhiteSpace(dto.Password) ? null : _passwordHasher.HashPassword(dto.Password),
                approval_type_id = BCIConst.ApprovalType.MODIFIED,
                is_active = dto.IsActive,
                is_deleted = false,
                created_by = actor,
                created_date = now,
            });

            await _context.SaveChangesAsync();
            return Success("User update request sent for approval.");
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
            var user = await _context.um_user_app.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
            if (user == null)
            {
                return Fail("User not found.");
            }

            if (await _context.temp_user_app_pending.AnyAsync(e => e.user_id == userId))
            {
                return Fail("User already has a pending request.");
            }

            var role = await _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId);
            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _context.temp_user_app_pending.AddAsync(new temp_user_app_pending
            {
                user_id = userId,
                full_name = user.full_name,
                email = user.email,
                phone_number = user.phone_number,
                kode_cabang = user.kode_cabang,
                gender_id = user.gender_id,
                nomor_induk_karyawan = user.nomor_induk_karyawan,
                group_id = role?.group_id,
                approval_type_id = BCIConst.ApprovalType.DELETE,
                is_active = false,
                is_deleted = true,
                created_by = actor,
                created_date = now,
            });

            await _context.SaveChangesAsync();
            return Success("User deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UnlockAsync(string userId)
    {
        var flag = await _context.um_user_app_flag.FirstOrDefaultAsync(e => e.user_id == userId);
        if (flag == null)
        {
            return Fail("User flag not found.");
        }

        flag.is_locked = false;
        flag.failed_password_attempt_count = 0;
        flag.failed_password_attempt_date = null;
        flag.last_locked_out_date = null;
        await _context.SaveChangesAsync();
        return Success("User unlocked successfully.");
    }

    public async Task<BaseGeneralResponse> ResetPasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return Fail("Password must be at least 8 characters.");
        }

        var user = await _context.um_user_app.FirstOrDefaultAsync(e => e.user_id == userId && e.isDeleted != true);
        if (user == null)
        {
            return Fail("User not found.");
        }

        user.password = _passwordHasher.HashPassword(newPassword);
        user.last_password_change_date = DateTime.Now;

        var flag = await _context.um_user_app_flag.FirstOrDefaultAsync(e => e.user_id == userId);
        if (flag != null)
        {
            flag.is_first_login = true;
            flag.is_locked = false;
            flag.failed_password_attempt_count = 0;
        }

        await _context.SaveChangesAsync();
        return Success("Password reset successfully.");
    }

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

public class DropDownService : IDropDownService
{
    private readonly __PROJECT__DbContext _context;

    public DropDownService(__PROJECT__DbContext context) => _context = context;

    public Task<List<DropdownItemDto>> GetGroupsAsync()
        => _context.um_group_app
            .Where(e => e.isActive != false && e.isDeleted != true)
            .OrderBy(e => e.group_name)
            .Select(e => new DropdownItemDto { Value = e.group_id, Text = e.group_name ?? e.group_id })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetGendersAsync()
        => _context.enum_gender
            .OrderBy(e => e.gender_code)
            .Select(e => new DropdownItemDto { Value = e.gender_code, Text = e.gender_name ?? e.gender_code })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetUsersAsync()
        => _context.um_user_app
            .Where(e => e.isActive != false && e.isDeleted != true)
            .OrderBy(e => e.user_id)
            .Select(e => new DropdownItemDto { Value = e.user_id, Text = e.full_name ?? e.user_id })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetMenuIconsAsync()
        => _context.enum_menu_icon
            .OrderBy(e => e.menu_icon_code)
            .Select(e => new DropdownItemDto
            {
                Value = e.menu_icon_code,
                Text = e.menu_icon_name_alternate ?? e.menu_icon_name ?? e.menu_icon_code,
            })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetMenuParentLevelsAsync()
        => _context.enum_menu_parent_level
            .OrderBy(e => e.parent_level_code)
            .Select(e => new DropdownItemDto
            {
                Value = e.parent_level_code,
                Text = e.parent_level_name ?? e.parent_level_code,
            })
            .ToListAsync();
}
