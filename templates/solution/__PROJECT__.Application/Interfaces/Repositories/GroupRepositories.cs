using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmGroupAppRepository
{
    Task<um_group_app?> GetByIdAsync(string groupId, CancellationToken cancellationToken = default);
    Task<um_group_app?> GetActiveByIdAsync(string groupId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAnyAsync(string groupId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(string groupId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveUsersAsync(string groupId, CancellationToken cancellationToken = default);
    Task AddAsync(um_group_app entity, CancellationToken cancellationToken = default);
}

public interface ITempGroupAppPendingRepository
{
    Task<temp_group_app_pending?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string groupId, CancellationToken cancellationToken = default);
    Task AddAsync(temp_group_app_pending entity, CancellationToken cancellationToken = default);
    void Remove(temp_group_app_pending entity);
}
