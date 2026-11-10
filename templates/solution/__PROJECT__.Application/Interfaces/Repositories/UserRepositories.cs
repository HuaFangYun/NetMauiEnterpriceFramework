using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmUserAppRepository
{
    Task<um_user_app?> GetByIdAsync(string userId);
    Task AddAsync(um_user_app entity);
    Task UpdateAsync(um_user_app entity);
}

public interface IUmUserGroupRepository
{
    Task<um_user_group?> GetByIdAsync(string userId);
    Task<string?> GetGroupNameAsync(string groupId);
}

public interface IUmUserAppFlagRepository
{
    Task<um_user_app_flag?> GetByUserIdAsync(string userId);
    Task AddAsync(um_user_app_flag entity);
    Task UpdateAsync(um_user_app_flag entity);
}

public interface IRefreshTokenRepository
{
    Task<um_user_refresh_token?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(um_user_refresh_token entity);
    Task UpdateAsync(um_user_refresh_token entity);
    Task RevokeAllUserTokensAsync(string userId);
}
