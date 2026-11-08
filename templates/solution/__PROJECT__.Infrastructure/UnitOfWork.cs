using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Repositories;
using __PROJECT__.Infrastructure.Data;
using __PROJECT__.Infrastructure.Repositories.Auth;

namespace __PROJECT__.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly __PROJECT__DbContext _context;

    private IUmUserAppRepository? _umUserApp;
    private IUmUserGroupRepository? _umUserGroup;
    private IUmUserAppFlagRepository? _umUserAppFlag;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(__PROJECT__DbContext context) => _context = context;

    public IUmUserAppRepository UmUserApp => _umUserApp ??= new UmUserAppRepository(_context);
    public IUmUserGroupRepository UmUserGroup => _umUserGroup ??= new UmUserGroupRepository(_context);
    public IUmUserAppFlagRepository UmUserAppFlag => _umUserAppFlag ??= new UmUserAppFlagRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
