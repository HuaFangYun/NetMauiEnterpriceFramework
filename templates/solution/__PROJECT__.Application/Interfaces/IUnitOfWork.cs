using __PROJECT__.Application.Interfaces.Repositories;

namespace __PROJECT__.Application.Interfaces;

public interface IUnitOfWork
{
    IUmUserAppRepository UmUserApp { get; }
    IUmUserGroupRepository UmUserGroup { get; }
    IUmUserAppFlagRepository UmUserAppFlag { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync();
}
