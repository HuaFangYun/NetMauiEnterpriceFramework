using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface ITempUserAppPendingRepository
{
    Task<temp_user_app_pending?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(temp_user_app_pending entity, CancellationToken cancellationToken = default);
    void Remove(temp_user_app_pending entity);
}
