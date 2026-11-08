namespace __PROJECT__.Shared.Services.Interfaces;

public interface ITokenStorage
{
    Task<string?> GetAccessTokenAsync();
    Task SetAccessTokenAsync(string token);
    Task<string?> GetRefreshTokenAsync();
    Task SetRefreshTokenAsync(string token);
    Task RemoveTokensAsync();
}

public interface INotifyAuthenticationState
{
    void NotifyUserLogin(string userId);
    void NotifyUserLogout();
}
