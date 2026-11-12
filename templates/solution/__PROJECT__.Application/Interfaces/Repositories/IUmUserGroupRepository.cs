using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmUserGroupRepository
{
    Task<um_user_group?> GetByIdAsync(string userId);
    Task<um_user_group?> GetActiveByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByGroupAsync(string groupId, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveAdministratorUserIdsAsync(CancellationToken cancellationToken = default);
    Task<string?> GetGroupNameAsync(string groupId);
    Task AddAsync(um_user_group entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(um_user_group entity);
}
