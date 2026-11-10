using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.Export;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Services;

public class ExportApiService
{
    private readonly ApiService _api;
    private readonly ITokenStorage _tokenStorage;

    public ExportApiService(ApiService api, ITokenStorage tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
    }

    public Task<ExportQueueResultDto?> QueueReportAsync(string reportId, DynamicLoadRequestDto request)
        => _api.PostFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/report/{Uri.EscapeDataString(reportId)}", request);

    public Task<ExportQueueResultDto?> QueueScreenAsync(string screenId, DynamicLoadRequestDto request)
        => _api.PostFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/screen/{Uri.EscapeDataString(screenId)}", request);

    public Task<ExportQueueResultDto?> QueuePdfReportAsync(string reportId, DynamicLoadRequestDto request)
        => _api.PostFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/pdf/report/{Uri.EscapeDataString(reportId)}", request);

    public Task<ExportQueueResultDto?> QueuePdfScreenAsync(string screenId, DynamicLoadRequestDto request)
        => _api.PostFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/pdf/screen/{Uri.EscapeDataString(screenId)}", request);

    public async Task<string?> ExportManagementAsync(string endpoint, string? search, string? tableName = null)
    {
        var baseAddress = _api.ApiBaseAddress;
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return null;
        }

        var url = string.IsNullOrWhiteSpace(tableName)
            ? endpoint
            : $"{endpoint}/{Uri.EscapeDataString(tableName)}";

        var queued = await _api.PostFromJsonAsync<DataTableRequestDto, ExportQueueResultDto>(
            url,
            new DataTableRequestDto { SearchTerm = search });

        if (queued == null || string.IsNullOrWhiteSpace(queued.JobId))
        {
            return null;
        }

        for (var attempt = 0; attempt < 60; attempt++)
        {
            var status = await _api.GetFromJsonAsync<ExportStatusDto>(
                $"api/export/status/{Uri.EscapeDataString(queued.JobId)}",
                unwrap: true);

            if (status?.Ready == true)
            {
                var token = await _tokenStorage.GetAccessTokenAsync();
                var query = $"?access_token={Uri.EscapeDataString(token ?? string.Empty)}";
                return $"{baseAddress}/api/export/download-export/{Uri.EscapeDataString(queued.JobId)}{query}";
            }

            await Task.Delay(500);
        }

        return null;
    }
}
