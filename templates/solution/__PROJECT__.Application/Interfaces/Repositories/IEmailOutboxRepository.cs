using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IEmailOutboxRepository
{
    Task AddAsync(tb_email_outbox entity, CancellationToken cancellationToken = default);
    Task<List<tb_email_outbox>> GetPendingAsync(
        DateTime now,
        int maxItems,
        CancellationToken cancellationToken = default);
}
