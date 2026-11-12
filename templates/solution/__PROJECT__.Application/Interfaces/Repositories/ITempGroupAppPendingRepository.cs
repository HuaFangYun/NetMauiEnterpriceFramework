using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface ITempGroupAppPendingRepository
{
    Task<temp_group_app_pending?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string groupId, CancellationToken cancellationToken = default);
    Task AddAsync(temp_group_app_pending entity, CancellationToken cancellationToken = default);
    void Remove(temp_group_app_pending entity);
}
