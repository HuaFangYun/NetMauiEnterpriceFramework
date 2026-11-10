using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;

using __PROJECT__.Application.DTOs.Auth;
using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;

    public AuthService(HttpClient httpClient, ITokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        }

        throw new Exception(await ReadErrorAsync(response));
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken });
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        await AttachTokenAsync(request);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserResponseDto>();
    }

    public async Task<UserResponseDto?> UpdateProfileAsync(UpdateProfileDto request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, "api/auth/profile")
        {
            Content = JsonContent.Create(request),
        };
        await AttachTokenAsync(message);

        var response = await _httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await ReadErrorAsync(response));
        }

        return await response.Content.ReadFromJsonAsync<UserResponseDto>();
    }

    public async Task ChangePasswordAsync(ChangePasswordDto request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-password")
        {
            Content = JsonContent.Create(request),
        };
        await AttachTokenAsync(message);

        var response = await _httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await ReadErrorAsync(response));
        }
    }

    public async Task LogoutAsync(string? refreshToken)
    {
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
            {
                Content = JsonContent.Create(new { refreshToken }),
            };
            await AttachTokenAsync(message);
            await _httpClient.SendAsync(message);
        }
        catch
        {
            // Local logout still proceeds.
        }
        finally
        {
            await _tokenStorage.RemoveTokensAsync();
        }
    }

    private async Task AttachTokenAsync(HttpRequestMessage request)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "Invalid Username / Password.";
        }

        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return response.ReasonPhrase ?? "Request failed.";
        }

        try
        {
            var json = System.Text.Json.JsonDocument.Parse(content);
            if (json.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? content;
            }
        }
        catch
        {
            // Keep raw content.
        }

        return content;
    }
}
