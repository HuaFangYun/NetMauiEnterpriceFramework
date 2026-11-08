using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.Approval;
using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Shared.Services;

public class ApprovalApiService
{
    private readonly ApiService _api;

    public ApprovalApiService(ApiService api) => _api = api;

    public async Task<GeneralListResponseDto<UserApprovalListItemDto>> GetPendingUsersAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<UserApprovalListItemDto>>("api/approval/users/list", request)
           ?? new GeneralListResponseDto<UserApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveUserAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/users/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectUserAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/users/{id}/reject", new { }));

    public async Task<GeneralListResponseDto<GroupApprovalListItemDto>> GetPendingGroupsAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<GroupApprovalListItemDto>>("api/approval/groups/list", request)
           ?? new GeneralListResponseDto<GroupApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveGroupAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/groups/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectGroupAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/groups/{id}/reject", new { }));

    public async Task<GeneralListResponseDto<RoleApprovalListItemDto>> GetPendingRolesAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<RoleApprovalListItemDto>>("api/approval/roles/list", request)
           ?? new GeneralListResponseDto<RoleApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveRoleAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/approval/roles/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectRoleAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/approval/roles/{id}/reject", new { }));

    public async Task<GeneralListResponseDto<MenuApprovalListItemDto>> GetPendingMenusAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<MenuApprovalListItemDto>>("api/approval/menus/list", request)
           ?? new GeneralListResponseDto<MenuApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveMenuAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/approval/menus/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectMenuAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/approval/menus/{id}/reject", new { }));

    private static async Task<BaseGeneralResponse> ReadAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<BaseGeneralResponse>()
                   ?? new BaseGeneralResponse { Success = true };
        }

        var error = await response.Content.ReadFromJsonAsync<BaseGeneralResponse>();
        return error ?? new BaseGeneralResponse
        {
            Success = false,
            Message = $"Request failed ({(int)response.StatusCode}).",
        };
    }
}
