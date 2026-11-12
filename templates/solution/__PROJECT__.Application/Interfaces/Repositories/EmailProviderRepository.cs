using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IEmailProviderRepository
{
    Task<tb_email_provider?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<tb_email_provider?> GetActiveProviderAsync(CancellationToken cancellationToken = default);
    Task<List<tb_email_provider>> GetActiveProvidersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(tb_email_provider entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(tb_email_provider entity);
}
