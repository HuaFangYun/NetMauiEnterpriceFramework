using Microsoft.JSInterop;

using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Services;

public class TokenStorage : ITokenStorage
{
    private const string AccessTokenKey = "pc_access_token";
    private const string RefreshTokenKey = "pc_refresh_token";
    private const string RememberMeKey = "pc_remember_me";
    private readonly IJSRuntime _jsRuntime;

    public TokenStorage(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public async Task<string?> GetAccessTokenAsync()
        => await GetTokenAsync(AccessTokenKey);

    public async Task SetAccessTokenAsync(string token)
        => await SetTokenAsync(AccessTokenKey, token);

    public async Task<string?> GetRefreshTokenAsync()
        => await GetTokenAsync(RefreshTokenKey);

    public async Task SetRefreshTokenAsync(string token)
        => await SetTokenAsync(RefreshTokenKey, token);

    public async Task SetRememberMeAsync(bool remember)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RememberMeKey, remember ? "1" : "0");
        await RemoveTokenKeysAsync();
    }

    public async Task RemoveTokensAsync()
    {
        await RemoveTokenKeysAsync();
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RememberMeKey);
    }

    private async Task<bool> IsRememberedAsync()
        => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RememberMeKey) == "1";

    private async Task<string?> GetTokenAsync(string key)
    {
        var storage = await IsRememberedAsync() ? "localStorage" : "sessionStorage";
        var token = await _jsRuntime.InvokeAsync<string?>(storage + ".getItem", key);
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        var fallback = storage == "localStorage" ? "sessionStorage" : "localStorage";
        return await _jsRuntime.InvokeAsync<string?>(fallback + ".getItem", key);
    }

    private async Task SetTokenAsync(string key, string token)
    {
        var storage = await IsRememberedAsync() ? "localStorage" : "sessionStorage";
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
        await _jsRuntime.InvokeVoidAsync(storage + ".setItem", key, token);
    }

    private async Task RemoveTokenKeysAsync()
    {
        foreach (var storage in new[] { "localStorage", "sessionStorage" })
        {
            await _jsRuntime.InvokeVoidAsync(storage + ".removeItem", AccessTokenKey);
            await _jsRuntime.InvokeVoidAsync(storage + ".removeItem", RefreshTokenKey);
        }
    }
}
