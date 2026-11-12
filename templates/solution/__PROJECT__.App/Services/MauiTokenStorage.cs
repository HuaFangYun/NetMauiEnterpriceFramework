using Microsoft.Maui.Storage;

using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.App.Services;

public class MauiTokenStorage : ITokenStorage
{
    private const string AccessTokenKey = "pc_access_token";
    private const string RefreshTokenKey = "pc_refresh_token";
    private const string RememberMeKey = "pc_remember_me";

    public Task<string?> GetAccessTokenAsync()
        => SecureStorage.Default.GetAsync(AccessTokenKey);

    public Task SetAccessTokenAsync(string token)
        => SecureStorage.Default.SetAsync(AccessTokenKey, token);

    public Task<string?> GetRefreshTokenAsync()
        => SecureStorage.Default.GetAsync(RefreshTokenKey);

    public Task SetRefreshTokenAsync(string token)
        => SecureStorage.Default.SetAsync(RefreshTokenKey, token);

    public Task SetRememberMeAsync(bool remember)
        => SecureStorage.Default.SetAsync(RememberMeKey, remember ? "1" : "0");

    public Task RemoveTokensAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(RememberMeKey);
        return Task.CompletedTask;
    }
}
