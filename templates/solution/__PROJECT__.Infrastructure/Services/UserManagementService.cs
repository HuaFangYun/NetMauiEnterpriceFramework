using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserQueryService _queryService;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;

    public UserManagementService(
        IUnitOfWork unitOfWork,
        IUserQueryService queryService,
        IPasswordHasherService passwordHasher,
        ICurrentUserService currentUser,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _errorLog = errorLog;
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("UserManagement", "ERROR", exception);

    public Task<GeneralListResponseDto<UserListItemDto>> GetListAsync(DataTableRequestDto request)
        => _queryService.GetListAsync(request);

    public Task<GeneralListResponseDto<UserFlagDto>> GetUserFlagListAsync(DataTableRequestDto request)
        => _queryService.GetUserFlagListAsync(request);

    public async Task<UserEditDto?> GetByIdAsync(string userId)
    {
        var user = await _unitOfWork.UmUserApp.GetActiveByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new UserEditDto
        {
            UserId = user.user_id,
            FullName = user.full_name,
            Email = user.email,
            PhoneNumber = user.phone_number,
            GenderId = user.gender_id,
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

            if (await _unitOfWork.UmUserApp.ExistsAnyAsync(dto.UserId))
            {
                return Fail("User ID already exists.");
            }

            if (await _unitOfWork.TempUserAppPending.ExistsAsync(dto.UserId))
            {
                return Fail("User already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _unitOfWork.TempUserAppPending.AddAsync(new temp_user_app_pending
            {
                user_id = dto.UserId,
                full_name = dto.FullName,
                email = dto.Email,
                phone_number = dto.PhoneNumber,
                gender_id = dto.GenderId,
                password = _passwordHasher.HashPassword(dto.Password),
                approval_type_id = Const.ApprovalType.ADD,
                is_active = dto.IsActive,
                is_deleted = false,
                created_by = actor,
                created_date = now,
            });

            await _unitOfWork.SaveChangesAsync();
            return Success("User creation request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(string userId, UserEditDto dto)
    {
        try
        {
            var user = await _unitOfWork.UmUserApp.GetActiveByIdAsync(userId);
            if (user == null)
            {
                return Fail("User not found.");
            }

            if (await _unitOfWork.TempUserAppPending.ExistsAsync(userId))
            {
                return Fail("User already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _unitOfWork.TempUserAppPending.AddAsync(new temp_user_app_pending
            {
                user_id = userId,
                full_name = dto.FullName,
                email = dto.Email,
                phone_number = dto.PhoneNumber,
                gender_id = dto.GenderId,
                password = string.IsNullOrWhiteSpace(dto.Password) ? null : _passwordHasher.HashPassword(dto.Password),
                approval_type_id = Const.ApprovalType.MODIFIED,
                is_active = dto.IsActive,
                is_deleted = false,
                created_by = actor,
                created_date = now,
            });

            await _unitOfWork.SaveChangesAsync();
            return Success("User update request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string userId)
    {
        try
        {
            var user = await _unitOfWork.UmUserApp.GetActiveByIdAsync(userId);
            if (user == null)
            {
                return Fail("User not found.");
            }

            if (await _unitOfWork.TempUserAppPending.ExistsAsync(userId))
            {
                return Fail("User already has a pending request.");
            }

            var role = await _unitOfWork.UmUserGroup.GetByIdAsync(userId);
            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _unitOfWork.TempUserAppPending.AddAsync(new temp_user_app_pending
            {
                user_id = userId,
                full_name = user.full_name,
                email = user.email,
                phone_number = user.phone_number,
                gender_id = user.gender_id,
                group_id = role?.group_id,
                approval_type_id = Const.ApprovalType.DELETE,
                is_active = false,
                is_deleted = true,
                created_by = actor,
                created_date = now,
            });

            await _unitOfWork.SaveChangesAsync();
            return Success("User deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UnlockAsync(string userId)
    {
        var flag = await _unitOfWork.UmUserAppFlag.GetByUserIdAsync(userId);
        if (flag == null)
        {
            return Fail("User flag not found.");
        }

        flag.is_locked = false;
        flag.failed_password_attempt_count = 0;
        flag.failed_password_attempt_date = null;
        flag.last_locked_out_date = null;
        await _unitOfWork.SaveChangesAsync();
        return Success("User unlocked successfully.");
    }

    public async Task<BaseGeneralResponse> ResetPasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return Fail("Password must be at least 8 characters.");
        }

        var user = await _unitOfWork.UmUserApp.GetActiveByIdAsync(userId);
        if (user == null)
        {
            return Fail("User not found.");
        }

        user.password = _passwordHasher.HashPassword(newPassword);
        user.last_password_change_date = DateTime.Now;

        var flag = await _unitOfWork.UmUserAppFlag.GetByUserIdAsync(userId);
        if (flag != null)
        {
            flag.is_first_login = true;
            flag.is_locked = false;
            flag.failed_password_attempt_count = 0;
        }

        await _unitOfWork.SaveChangesAsync();
        return Success("Password reset successfully.");
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
