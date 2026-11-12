using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmGroupMenuRepository
{
    Task<List<um_group_menu>> GetByGroupAsync(string groupId, CancellationToken cancellationToken = default);
    Task<List<um_group_menu>> GetByMenuIdAsync(long menuId, CancellationToken cancellationToken = default);
    Task AddAsync(um_group_menu entity, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<um_group_menu> entities);
}
