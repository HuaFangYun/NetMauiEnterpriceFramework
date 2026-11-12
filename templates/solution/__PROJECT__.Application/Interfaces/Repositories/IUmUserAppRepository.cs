using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmUserAppRepository
{
    Task<um_user_app?> GetByIdAsync(string userId);
    Task<um_user_app?> FindByIdOrEmailAsync(string identifier, CancellationToken cancellationToken = default);
    Task<um_user_app?> GetActiveByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAnyAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(um_user_app entity);
    Task UpdateAsync(um_user_app entity);
}
