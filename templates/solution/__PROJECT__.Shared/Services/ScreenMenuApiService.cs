using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Screen;

namespace __PROJECT__.Shared.Services;

public class ScreenMenuApiService
{
    private readonly ApiService _api;

    public ScreenMenuApiService(ApiService api) => _api = api;

    public async Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, GeneralListResponseDto<ScreenMenuListItemDto>>(
               "api/screen-menu-editor/list", request)
           ?? new GeneralListResponseDto<ScreenMenuListItemDto>();

    public Task<ScreenMenuEditDto?> GetAsync(string id)
        => _api.GetFromJsonAsync<ScreenMenuEditDto>($"api/screen-menu-editor/{Uri.EscapeDataString(id)}");

    public async Task<BaseGeneralResponse> StageAsync(ScreenMenuEditDto dto)
        => await ReadAsync(await _api.PostAsync("api/screen-menu-editor", dto));

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
