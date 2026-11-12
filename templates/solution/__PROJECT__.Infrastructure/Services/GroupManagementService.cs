using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Group;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Infrastructure.Services;

public class GroupManagementService : IGroupManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGroupQueryService _queryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;

    public GroupManagementService(
        IUnitOfWork unitOfWork,
        IGroupQueryService queryService,
        ICurrentUserService currentUser,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _currentUser = currentUser;
        _errorLog = errorLog;
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("GroupManagement", "ERROR", exception);

    public Task<GeneralListResponseDto<GroupListItemDto>> GetListAsync(DataTableRequestDto request)
        => _queryService.GetListAsync(request);

    public async Task<GroupEditDto?> GetByIdAsync(string groupId)
    {
        var group = await _unitOfWork.UmGroupApp.GetActiveByIdAsync(groupId);
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

            if (await _unitOfWork.UmGroupApp.ExistsAnyAsync(dto.GroupId))
            {
                return Fail("Group ID already exists.");
            }

            if (await _unitOfWork.TempGroupAppPending.ExistsAsync(dto.GroupId))
            {
                return Fail("Group already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _unitOfWork.TempGroupAppPending.AddAsync(new temp_group_app_pending
            {
                group_id = dto.GroupId,
                group_name = dto.GroupName ?? string.Empty,
                approval_type_id = Const.ApprovalType.ADD,
                is_active = dto.IsActive,
                created_by = actor,
                created_date = now,
            });

            await _unitOfWork.SaveChangesAsync();
            return Success("Group creation request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(string groupId, GroupEditDto dto)
    {
        try
        {
            var group = await _unitOfWork.UmGroupApp.GetActiveByIdAsync(groupId);
            if (group == null)
            {
                return Fail("Group not found.");
            }

            if (await _unitOfWork.TempGroupAppPending.ExistsAsync(groupId))
            {
                return Fail("Group already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _unitOfWork.TempGroupAppPending.AddAsync(new temp_group_app_pending
            {
                group_id = groupId,
                group_name = dto.GroupName ?? string.Empty,
                approval_type_id = Const.ApprovalType.MODIFIED,
                is_active = dto.IsActive,
                created_by = actor,
                created_date = now,
            });

            await _unitOfWork.SaveChangesAsync();
            return Success("Group update request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string groupId)
    {
        try
        {
            var group = await _unitOfWork.UmGroupApp.GetActiveByIdAsync(groupId);
            if (group == null)
            {
                return Fail("Group not found.");
            }

            if (groupId == Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE)
            {
                return Fail("Administrator group cannot be deleted.");
            }

            if (await _unitOfWork.UmGroupApp.HasActiveUsersAsync(groupId))
            {
                return Fail("Group is still assigned to one or more users.");
            }

            if (await _unitOfWork.TempGroupAppPending.ExistsAsync(groupId))
            {
                return Fail("Group already has a pending request.");
            }

            var now = DateTime.Now;
            var actor = _currentUser.UserId ?? "system";

            await _unitOfWork.TempGroupAppPending.AddAsync(new temp_group_app_pending
            {
                group_id = groupId,
                group_name = group.group_name ?? string.Empty,
                approval_type_id = Const.ApprovalType.DELETE,
                is_active = false,
                created_by = actor,
                created_date = now,
            });

            await _unitOfWork.SaveChangesAsync();
            return Success("Group deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
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
