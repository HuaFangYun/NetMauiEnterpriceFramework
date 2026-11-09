using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Parameter;

namespace __PROJECT__.Shared.Services;

public class ParameterApiService
{
    private readonly ApiService _api;

    public ParameterApiService(ApiService api) => _api = api;

    public async Task<List<ParameterCardDto>> GetCardsAsync()
        => await _api.GetFromJsonAsync<List<ParameterCardDto>>("api/parameter/cards") ?? new List<ParameterCardDto>();

    public async Task<List<ParameterFieldDto>> GetFieldsAsync(string tableName)
        => await _api.GetFromJsonAsync<List<ParameterFieldDto>>($"api/parameter/{Uri.EscapeDataString(tableName)}/fields") ?? new List<ParameterFieldDto>();

    public async Task<Dictionary<string, List<DropdownItemDto>>> GetOptionsAsync(string tableName)
        => await _api.GetFromJsonAsync<Dictionary<string, List<DropdownItemDto>>>(
               $"api/parameter/{Uri.EscapeDataString(tableName)}/options")
           ?? new Dictionary<string, List<DropdownItemDto>>(StringComparer.OrdinalIgnoreCase);

    public async Task<ParameterListResponseDto> GetListAsync(string tableName, DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, ParameterListResponseDto>(
               $"api/parameter/{Uri.EscapeDataString(tableName)}/list", request)
           ?? new ParameterListResponseDto();

    public Task<Dictionary<string, string>?> GetAsync(string tableName, string? key)
        => _api.GetFromJsonAsync<Dictionary<string, string>>(
            $"api/parameter/{Uri.EscapeDataString(tableName)}/detail?key={Uri.EscapeDataString(key ?? string.Empty)}");

    public async Task<BaseGeneralResponse> SaveAsync(ParameterSaveRequestDto request)
        => await ReadAsync(await _api.PostAsync("api/parameter/save", request));

    public async Task<BaseGeneralResponse> DeleteAsync(string tableName, string? key)
        => await ReadAsync(await _api.DeleteAsync(
            $"api/parameter/{Uri.EscapeDataString(tableName)}?key={Uri.EscapeDataString(key ?? string.Empty)}"));

    public async Task<GeneralListResponseDto<ParameterPendingListItemDto>> GetPendingAsync(ParameterPendingRequestDto request)
        => await _api.PostRawFromJsonAsync<ParameterPendingRequestDto, GeneralListResponseDto<ParameterPendingListItemDto>>(
               "api/parameter/pending/list", request)
           ?? new GeneralListResponseDto<ParameterPendingListItemDto>();

    public async Task<BaseGeneralResponse> ApproveAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/parameter/pending/{id}/approve", new { }));

    public async Task<BaseGeneralResponse> RejectAsync(Guid id)
        => await ReadAsync(await _api.PostAsync($"api/parameter/pending/{id}/reject", new { }));

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
