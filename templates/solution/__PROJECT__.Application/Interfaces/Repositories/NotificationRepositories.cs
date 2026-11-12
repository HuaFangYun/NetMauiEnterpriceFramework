using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface ITbNotificationRepository
{
    Task<tb_notification?> GetByIdForUserAsync(long id, string userId, CancellationToken cancellationToken = default);
    Task<List<tb_notification>> GetUnseenByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(tb_notification entity, CancellationToken cancellationToken = default);
}

public interface ITbNotificationTypeRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(tb_notification_type entity, CancellationToken cancellationToken = default);
}
