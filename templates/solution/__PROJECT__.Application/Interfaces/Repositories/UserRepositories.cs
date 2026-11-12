using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmUserAppRepository
{
    Task<um_user_app?> GetByIdAsync(string userId);
    Task<um_user_app?> FindByIdOrEmailAsync(string identifier, CancellationToken cancellationToken = default);
    Task<um_user_app?> GetActiveByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAnyAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(um_user_app entity);
    Task UpdateAsync(um_user_app entity);
}

public interface ITempUserAppPendingRepository
{
    Task<temp_user_app_pending?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(temp_user_app_pending entity, CancellationToken cancellationToken = default);
    void Remove(temp_user_app_pending entity);
}

public interface IUmUserGroupRepository
{
    Task<um_user_group?> GetByIdAsync(string userId);
    Task<um_user_group?> GetActiveByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByGroupAsync(string groupId, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveAdministratorUserIdsAsync(CancellationToken cancellationToken = default);
    Task<string?> GetGroupNameAsync(string groupId);
    Task AddAsync(um_user_group entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(um_user_group entity);
}

public interface IUmUserAppSessionRepository
{
    Task UpsertAsync(string userId, string? sessionId);
    Task RemoveAsync(string userId);
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
