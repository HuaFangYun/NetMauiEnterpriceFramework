using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<um_user_refresh_token?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(um_user_refresh_token entity);
    Task UpdateAsync(um_user_refresh_token entity);
    Task RevokeAllUserTokensAsync(string userId);
}
