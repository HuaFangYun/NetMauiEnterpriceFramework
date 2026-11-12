using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Components;

using __PROJECT__.Application.DTOs.Auth;
using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Handlers;

public class AuthenticatingHttpHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private readonly ITokenStorage _tokenStorage;
    private readonly NavigationManager _navigation;

    public AuthenticatingHttpHandler(ITokenStorage tokenStorage, NavigationManager navigation)
    {
        _tokenStorage = tokenStorage;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri != null && !IsAuthEndpoint(request.RequestUri))
        {
            await RefreshIfNeededAsync(request.RequestUri, cancellationToken);
        }

        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokenStorage.RemoveTokensAsync();
            if (!_navigation.Uri.EndsWith('/'))
            {
                _navigation.NavigateTo("/", true);
            }
        }

        return response;
    }

    private async Task RefreshIfNeededAsync(Uri requestUri, CancellationToken cancellationToken)
    {
        var accessToken = await _tokenStorage.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(accessToken) || !IsExpiring(accessToken))
        {
            return;
        }

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            accessToken = await _tokenStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken) || !IsExpiring(accessToken))
            {
                return;
            }

            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await _tokenStorage.RemoveTokensAsync();
                return;
            }

            var refreshUri = new Uri(requestUri.GetLeftPart(UriPartial.Authority) + "/api/auth/refresh");
            using var client = new HttpClient();
            using var response = await client.PostAsJsonAsync(refreshUri, new { refreshToken }, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await _tokenStorage.RemoveTokensAsync();
                return;
            }

            var refreshed = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken);
            if (refreshed == null || string.IsNullOrWhiteSpace(refreshed.AccessToken))
            {
                await _tokenStorage.RemoveTokensAsync();
                return;
            }

            await _tokenStorage.SetAccessTokenAsync(refreshed.AccessToken);
            await _tokenStorage.SetRefreshTokenAsync(refreshed.RefreshToken);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static bool IsExpiring(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow.AddSeconds(30);
        }
        catch
        {
            return true;
        }
    }

    private static bool IsAuthEndpoint(Uri uri)
        => uri.AbsolutePath.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase)
           || uri.AbsolutePath.Contains("/api/auth/refresh", StringComparison.OrdinalIgnoreCase);
}
