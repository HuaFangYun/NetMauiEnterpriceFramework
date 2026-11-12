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
