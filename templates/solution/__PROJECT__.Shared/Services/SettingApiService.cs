using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Group;
using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.DTOs.User;

namespace __PROJECT__.Shared.Services;

public class SettingApiService
{
    private readonly ApiService _api;

    public SettingApiService(ApiService api) => _api = api;

    public async Task<GeneralListResponseDto<UserListItemDto>> GetUsersAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<UserListItemDto>>("api/user/list", request)
           ?? new GeneralListResponseDto<UserListItemDto>();

    public async Task<GeneralListResponseDto<UserFlagDto>> GetUserFlagsAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<UserFlagDto>>("api/user/list-user-flag", request)
           ?? new GeneralListResponseDto<UserFlagDto>();

    public async Task<BaseGeneralResponse> ResetUserPasswordAsync(string userId, string newPassword)
        => await ReadAsync(await _api.PostAsync("api/user/reset-password", new UserActionDto
        {
            UserId = userId,
            NewPassword = newPassword,
        }));

    public Task<UserEditDto?> GetUserAsync(string userId)
        => _api.GetFromJsonAsync<UserEditDto>($"api/user/{Uri.EscapeDataString(userId)}");

    public async Task<BaseGeneralResponse> CreateUserAsync(UserEditDto dto)
        => await ReadAsync(await _api.PostAsync("api/user", dto));

    public async Task<BaseGeneralResponse> UpdateUserAsync(string userId, UserEditDto dto)
        => await ReadAsync(await _api.PutAsync($"api/user/{Uri.EscapeDataString(userId)}", dto));

    public async Task<BaseGeneralResponse> DeleteUserAsync(string userId)
        => await ReadAsync(await _api.DeleteAsync($"api/user/{Uri.EscapeDataString(userId)}"));

    public async Task<BaseGeneralResponse> UnlockUserAsync(string userId)
        => await ReadAsync(await _api.PostAsync("api/user/unlock", new UserActionDto { UserId = userId }));

    public async Task<List<DropdownItemDto>> GetGroupsAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/groups") ?? new List<DropdownItemDto>();

    public async Task<List<DropdownItemDto>> GetGendersAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/genders") ?? new List<DropdownItemDto>();

    public async Task<GeneralListResponseDto<GroupListItemDto>> GetGroupsAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<GroupListItemDto>>("api/group/list", request)
           ?? new GeneralListResponseDto<GroupListItemDto>();

    public Task<GroupEditDto?> GetGroupAsync(string groupId)
        => _api.GetFromJsonAsync<GroupEditDto>($"api/group/{Uri.EscapeDataString(groupId)}");

    public async Task<BaseGeneralResponse> CreateGroupAsync(GroupEditDto dto)
        => await ReadAsync(await _api.PostAsync("api/group", dto));

    public async Task<BaseGeneralResponse> UpdateGroupAsync(string groupId, GroupEditDto dto)
        => await ReadAsync(await _api.PutAsync($"api/group/{Uri.EscapeDataString(groupId)}", dto));

    public async Task<BaseGeneralResponse> DeleteGroupAsync(string groupId)
        => await ReadAsync(await _api.DeleteAsync($"api/group/{Uri.EscapeDataString(groupId)}"));

    public async Task<GeneralListResponseDto<RoleListItemDto>> GetRolesAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<RoleListItemDto>>("api/usergroup/list", request)
           ?? new GeneralListResponseDto<RoleListItemDto>();

    public Task<RoleEditDto?> GetRoleAsync(string userId)
        => _api.GetFromJsonAsync<RoleEditDto>($"api/usergroup/{Uri.EscapeDataString(userId)}");

    public async Task<BaseGeneralResponse> CreateRoleAsync(RoleEditDto dto)
        => await ReadAsync(await _api.PostAsync("api/usergroup", dto));

    public async Task<BaseGeneralResponse> UpdateRoleAsync(string userId, RoleEditDto dto)
        => await ReadAsync(await _api.PutAsync($"api/usergroup/{Uri.EscapeDataString(userId)}", dto));

    public async Task<BaseGeneralResponse> DeleteRoleAsync(string userId)
        => await ReadAsync(await _api.DeleteAsync($"api/usergroup/{Uri.EscapeDataString(userId)}"));

    public async Task<List<DropdownItemDto>> GetUsersAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/users") ?? new List<DropdownItemDto>();

    public async Task<List<DropdownItemDto>> GetMenuIconsAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/menu-icons") ?? new List<DropdownItemDto>();

    public async Task<List<DropdownItemDto>> GetMenuParentLevelsAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/menu-parent-levels") ?? new List<DropdownItemDto>();

    public async Task<GeneralListResponseDto<MenuListItemDto>> GetMenusAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<MenuListItemDto>>("api/menu/list", request)
           ?? new GeneralListResponseDto<MenuListItemDto>();

    public Task<MenuEditDto?> GetMenuAsync(long id)
        => _api.GetFromJsonAsync<MenuEditDto>($"api/menu/detail/{id}");

    public async Task<BaseGeneralResponse> CreateMenuAsync(MenuEditDto dto)
        => await ReadAsync(await _api.PostAsync("api/menu", dto));

    public async Task<BaseGeneralResponse> UpdateMenuAsync(long id, MenuEditDto dto)
        => await ReadAsync(await _api.PutAsync($"api/menu/{id}", dto));

    public async Task<BaseGeneralResponse> DeleteMenuAsync(long id)
        => await ReadAsync(await _api.DeleteAsync($"api/menu/{id}"));

    public async Task<List<MenuTreeItemDto>> GetMenuTreeAsync()
        => await _api.GetFromJsonAsync<List<MenuTreeItemDto>>("api/menu/tree") ?? new List<MenuTreeItemDto>();

    public async Task<List<long>> GetAssignedMenusAsync(string groupId)
        => await _api.GetFromJsonAsync<List<long>>($"api/menu/assigned/{Uri.EscapeDataString(groupId)}") ?? new List<long>();

    public async Task<BaseGeneralResponse> SaveMenuMapAsync(MenuMapDto dto)
        => await ReadAsync(await _api.PostAsync("api/menu/save-map", dto));

    private static async Task<BaseGeneralResponse> ReadAsync(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentLength is null or 0)
        {
            return new BaseGeneralResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = response.ReasonPhrase ?? "Request failed.",
            };
        }

        var result = await response.Content.ReadFromJsonAsync<BaseGeneralResponse>();
        return result ?? new BaseGeneralResponse
        {
            Success = response.IsSuccessStatusCode,
            Message = response.ReasonPhrase ?? "Request failed.",
        };
    }
}
