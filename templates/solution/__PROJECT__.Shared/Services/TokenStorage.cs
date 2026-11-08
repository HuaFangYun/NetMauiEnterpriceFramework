using Microsoft.JSInterop;

using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Services;

public class TokenStorage : ITokenStorage
{
    private const string AccessTokenKey = "pc_access_token";
    private const string RefreshTokenKey = "pc_refresh_token";
    private readonly IJSRuntime _jsRuntime;

    public TokenStorage(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public Task<string?> GetAccessTokenAsync()
        => _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey).AsTask();

    public Task SetAccessTokenAsync(string token)
        => _jsRuntime.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, token).AsTask();

    public Task<string?> GetRefreshTokenAsync()
        => _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey).AsTask();

    public Task SetRefreshTokenAsync(string token)
        => _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, token).AsTask();

    public async Task RemoveTokensAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }
}
