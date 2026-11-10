using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.Interfaces.Repositories;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Repositories.Auth;

public class UmUserAppRepository : IUmUserAppRepository
{
    private readonly __PROJECT__DbContext _context;

    public UmUserAppRepository(__PROJECT__DbContext context) => _context = context;

    public Task<um_user_app?> GetByIdAsync(string userId)
        => _context.um_user_app.FirstOrDefaultAsync(e => e.user_id == userId);

    public Task AddAsync(um_user_app entity)
        => _context.um_user_app.AddAsync(entity).AsTask();

    public Task UpdateAsync(um_user_app entity)
    {
        _context.um_user_app.Update(entity);
        return Task.CompletedTask;
    }
}

public class UmUserGroupRepository : IUmUserGroupRepository
{
    private readonly __PROJECT__DbContext _context;

    public UmUserGroupRepository(__PROJECT__DbContext context) => _context = context;

    public Task<um_user_group?> GetByIdAsync(string userId)
        => _context.um_user_group.FirstOrDefaultAsync(e => e.user_id == userId);

    public Task<string?> GetGroupNameAsync(string groupId)
        => _context.um_group_app
            .Where(e => e.group_id == groupId && e.isDeleted != true)
            .Select(e => e.group_name)
            .FirstOrDefaultAsync();
}

public class UmUserAppFlagRepository : IUmUserAppFlagRepository
{
    private readonly __PROJECT__DbContext _context;

    public UmUserAppFlagRepository(__PROJECT__DbContext context) => _context = context;

    public Task<um_user_app_flag?> GetByUserIdAsync(string userId)
        => _context.um_user_app_flag.FirstOrDefaultAsync(e => e.user_id == userId);

    public Task AddAsync(um_user_app_flag entity)
        => _context.um_user_app_flag.AddAsync(entity).AsTask();

    public Task UpdateAsync(um_user_app_flag entity)
    {
        _context.um_user_app_flag.Update(entity);
        return Task.CompletedTask;
    }
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly __PROJECT__DbContext _context;

    public RefreshTokenRepository(__PROJECT__DbContext context) => _context = context;

    public Task<um_user_refresh_token?> GetByTokenHashAsync(string tokenHash)
        => _context.um_user_refresh_token
            .FirstOrDefaultAsync(e => e.refresh_token == tokenHash && !e.is_revoked);

    public Task AddAsync(um_user_refresh_token entity)
        => _context.um_user_refresh_token.AddAsync(entity).AsTask();

    public Task UpdateAsync(um_user_refresh_token entity)
    {
        _context.um_user_refresh_token.Update(entity);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var tokens = await _context.um_user_refresh_token
            .Where(e => e.user_id == userId && !e.is_revoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.is_revoked = true;
            token.modifiedDate = DateTime.Now;
        }
    }
}
