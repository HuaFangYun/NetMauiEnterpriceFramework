using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Components;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Services;

public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStorage _tokenStorage;
    private readonly NavigationManager _navigation;

    public ApiService(IHttpClientFactory httpClientFactory, ITokenStorage tokenStorage, NavigationManager navigation)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
        _navigation = navigation;
    }

    public string? ApiBaseAddress
        => _httpClientFactory.CreateClient("ApiClient").BaseAddress?.ToString().TrimEnd('/');

    public Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, url, null, cancellationToken);

    public Task<HttpResponseMessage> PostAsync<T>(string url, T data, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, url, data, cancellationToken);

    public Task<HttpResponseMessage> PutAsync<T>(string url, T data, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Put, url, data, cancellationToken);

    public Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Delete, url, null, cancellationToken);

    public async Task<HttpResponseMessage> PostFileAsync(
        string url,
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var token = await _tokenStorage.GetAccessTokenAsync();
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", fileName);

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await HandleUnauthorizedAsync();
        }

        return response;
    }

    public async Task<T?> GetFromJsonAsync<T>(string url, bool unwrap = false, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        if (unwrap)
        {
            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);
            if (wrapper == null || !wrapper.Success)
            {
                throw new Exception(wrapper?.Message ?? "Unknown error");
            }
            return wrapper.Data;
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PostFromJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cancellationToken = default)
    {
        var response = await PostAsync(url, data, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: cancellationToken);
        if (wrapper == null || !wrapper.Success)
        {
            throw new Exception(wrapper?.Message ?? "Unknown error");
        }

        return wrapper.Data;
    }

    public async Task<TResponse?> PostRawFromJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cancellationToken = default)
    {
        var response = await PostAsync(url, data, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PutFromJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cancellationToken = default)
    {
        var response = await PutAsync(url, data, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> DeleteFromJsonAsync<TResponse>(string url, CancellationToken cancellationToken = default)
    {
        var response = await DeleteAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? data, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var token = await _tokenStorage.GetAccessTokenAsync();
        using var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        if (data != null)
        {
            request.Content = JsonContent.Create(data);
        }

        var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await HandleUnauthorizedAsync();
        }

        return response;
    }

    private async Task HandleUnauthorizedAsync()
    {
        if (_navigation.Uri.EndsWith('/') || _navigation.Uri.Contains("/login", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _tokenStorage.RemoveTokensAsync();
        _navigation.NavigateTo("/", true);
    }
}
