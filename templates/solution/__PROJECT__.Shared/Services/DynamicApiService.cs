using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Shared.Services;

public class DynamicApiService
{
    private readonly ApiService _api;

    public DynamicApiService(ApiService api) => _api = api;

    public Task<DynamicScreenDto?> GetScreenAsync(string screenId)
        => _api.GetFromJsonAsync<DynamicScreenDto>($"api/dynamic/screen/{Uri.EscapeDataString(screenId)}");

    public async Task<DynamicLoadResultDto?> LoadScreenAsync(string screenId, DynamicLoadRequestDto request)
        => await _api.PostRawFromJsonAsync<DynamicLoadRequestDto, DynamicLoadResultDto>(
            $"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/load", request);

    public async Task<List<DynamicOptionDto>> GetOptionsAsync(string screenId, string fieldId)
        => await _api.GetFromJsonAsync<List<DynamicOptionDto>>(
               $"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/options/{Uri.EscapeDataString(fieldId)}")
           ?? new List<DynamicOptionDto>();

    public Task<Dictionary<string, object?>?> GetChildDetailAsync(string screenId, string key)
        => _api.GetFromJsonAsync<Dictionary<string, object?>>(
            $"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/child/detail?key={Uri.EscapeDataString(key)}");

    public async Task<BaseGeneralResponse> SaveChildAsync(string screenId, DynamicMutationRequestDto request)
        => await ReadAsync(await _api.PostAsync($"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/child/save", request));

    public async Task<BaseGeneralResponse> ProcessAsync(string screenId, DynamicMutationRequestDto request)
        => await ReadAsync(await _api.PostAsync($"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/process", request));

    public async Task<BaseGeneralResponse> DeleteAsync(string screenId, DynamicMutationRequestDto request)
        => await ReadAsync(await _api.PostAsync($"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/delete", request));

    public async Task<BaseGeneralResponse> ImportExcelAsync(string screenId, Stream stream, string fileName)
        => await ReadAsync(await _api.PostFileAsync(
            $"api/dynamic/screen/{Uri.EscapeDataString(screenId)}/import-excel", stream, fileName));

    public string? ApiBaseAddress => _api.ApiBaseAddress;

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

    public Task<DynamicReportDto?> GetReportAsync(string reportId)
        => _api.GetFromJsonAsync<DynamicReportDto>($"api/dynamic/report/{Uri.EscapeDataString(reportId)}");

    public async Task<DynamicLoadResultDto?> LoadReportAsync(string reportId, DynamicLoadRequestDto request)
        => await _api.PostRawFromJsonAsync<DynamicLoadRequestDto, DynamicLoadResultDto>(
            $"api/dynamic/report/{Uri.EscapeDataString(reportId)}/load", request);
}
