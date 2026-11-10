using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.Approval;
using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Shared.Services;

public class ApprovalApiService
{
    private readonly ApiService _api;

    public ApprovalApiService(ApiService api) => _api = api;

    public async Task<PaginatedResponse<UserApprovalListItemDto>> GetPendingUsersAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<UserApprovalListItemDto>>("api/approval/users/list", request)
           ?? new PaginatedResponse<UserApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveUserAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/users/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectUserAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/users/{id}/reject", new { }));

    public async Task<PaginatedResponse<GroupApprovalListItemDto>> GetPendingGroupsAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<GroupApprovalListItemDto>>("api/approval/groups/list", request)
           ?? new PaginatedResponse<GroupApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveGroupAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/groups/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectGroupAsync(long id)
        => await ReadAsync(await _api.PostAsync($"api/approval/groups/{id}/reject", new { }));

    public async Task<PaginatedResponse<RoleApprovalListItemDto>> GetPendingRolesAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<RoleApprovalListItemDto>>("api/approval/roles/list", request)
           ?? new PaginatedResponse<RoleApprovalListItemDto>();

    public async Task<BaseGeneralResponse> ApproveRoleAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/approval/roles/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectRoleAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/approval/roles/{id}/reject", new { }));

    public async Task<PaginatedResponse<MenuApprovalListItemDto>> GetPendingMenusAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<MenuApprovalListItemDto>>("api/approval/menus/list", request)
           ?? new PaginatedResponse<MenuApprovalListItemDto>();

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
