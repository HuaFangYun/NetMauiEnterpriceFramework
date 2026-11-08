using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.Export;

namespace __PROJECT__.Shared.Services;

public class ExportApiService
{
    private readonly ApiService _api;

    public ExportApiService(ApiService api) => _api = api;

    public Task<ExportQueueResultDto?> QueueReportAsync(string reportId, DynamicLoadRequestDto request)
        => _api.PostRawFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/report/{Uri.EscapeDataString(reportId)}", request);

    public Task<ExportQueueResultDto?> QueueScreenAsync(string screenId, DynamicLoadRequestDto request)
        => _api.PostRawFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/screen/{Uri.EscapeDataString(screenId)}", request);

    public Task<ExportQueueResultDto?> QueuePdfReportAsync(string reportId, DynamicLoadRequestDto request)
        => _api.PostRawFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/pdf/report/{Uri.EscapeDataString(reportId)}", request);

    public Task<ExportQueueResultDto?> QueuePdfScreenAsync(string screenId, DynamicLoadRequestDto request)
        => _api.PostRawFromJsonAsync<DynamicLoadRequestDto, ExportQueueResultDto>(
            $"api/export/pdf/screen/{Uri.EscapeDataString(screenId)}", request);
}
