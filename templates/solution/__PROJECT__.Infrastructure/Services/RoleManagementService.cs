using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Infrastructure.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoleQueryService _queryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IParameterStagingService _staging;
    private readonly IErrorLogService _errorLog;

    public RoleManagementService(
        IUnitOfWork unitOfWork,
        IRoleQueryService queryService,
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
        => _errorLog.LogAsync("RoleManagement", "ERROR", exception);

    public Task<GeneralListResponseDto<RoleListItemDto>> GetListAsync(DataTableRequestDto request)
        => _queryService.GetListAsync(request);

    public async Task<RoleEditDto?> GetByIdAsync(string userId)
    {
        var role = await _unitOfWork.UmUserGroup.GetActiveByIdAsync(userId);
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

            if (!await _unitOfWork.UmUserApp.ExistsActiveAsync(dto.UserId))
            {
                return Fail("User not found.");
            }

            if (!await _unitOfWork.UmGroupApp.ExistsActiveAsync(dto.GroupId))
            {
                return Fail("Group not found.");
            }

            if (await _unitOfWork.UmUserGroup.ExistsActiveByUserAsync(dto.UserId))
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
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(string userId, RoleEditDto dto)
    {
        try
        {
            var role = await _unitOfWork.UmUserGroup.GetActiveByIdAsync(userId);
            if (role == null)
            {
                return Fail("Role not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.GroupId)
                || !await _unitOfWork.UmGroupApp.ExistsActiveAsync(dto.GroupId))
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
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string userId)
    {
        try
        {
            var role = await _unitOfWork.UmUserGroup.GetActiveByIdAsync(userId);
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
            await LogErrorAsync(ex);
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
