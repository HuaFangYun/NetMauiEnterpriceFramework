using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<List<tb_password_reset_token>> GetUnusedByUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<tb_password_reset_token?> GetValidByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> AnyValidByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task AddAsync(tb_password_reset_token entity, CancellationToken cancellationToken = default);
}

public interface IEmailTemplateRepository
{
    Task<tb_email_template?> GetActiveByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default);
}

public interface IEmailOutboxRepository
{
    Task AddAsync(tb_email_outbox entity, CancellationToken cancellationToken = default);
    Task<List<tb_email_outbox>> GetPendingAsync(
        DateTime now,
        int maxItems,
        CancellationToken cancellationToken = default);
}
