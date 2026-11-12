using System.Net.Http.Json;

using __PROJECT__.Application.DTOs.Email;
using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Shared.Services;

public class EmailProviderApiService
{
    private readonly ApiService _api;

    public EmailProviderApiService(ApiService api) => _api = api;

    public async Task<PaginatedResponse<EmailProviderDto>> GetListAsync(DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<EmailProviderDto>>(
               "api/email-provider/list", request)
           ?? new PaginatedResponse<EmailProviderDto>();

    public Task<EmailProviderDto?> GetAsync(int id)
        => _api.GetFromJsonAsync<EmailProviderDto>($"api/email-provider/{id}", unwrap: true);

    public async Task<BaseGeneralResponse> SaveAsync(EmailProviderDto request)
        => await ReadAsync(await _api.PostAsync("api/email-provider", request));

    public async Task<BaseGeneralResponse> DeleteAsync(int id)
        => await ReadAsync(await _api.DeleteAsync($"api/email-provider/{id}"));

    public async Task<BaseGeneralResponse> TestAsync(int id, string? recipient)
    {
        var url = $"api/email-provider/{id}/test";
        if (!string.IsNullOrWhiteSpace(recipient))
        {
            url += $"?recipient={Uri.EscapeDataString(recipient)}";
        }

        return await ReadAsync(await _api.PostAsync(url, new { }));
    }

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
