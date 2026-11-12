using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmMenuRepository
{
    Task<um_menu?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<um_menu?> GetActiveByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<um_menu?> GetByUniqueAsync(string menuUnique, CancellationToken cancellationToken = default);
    Task<bool> AnyByUniqueAsync(string menuUnique, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByUniqueAsync(string menuUnique, CancellationToken cancellationToken = default);
    Task<bool> ExistsAnyByUniqueExceptIdAsync(string menuUnique, long id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveChildrenAsync(string menuUnique, CancellationToken cancellationToken = default);
    Task<List<um_menu>> GetByParentAsync(string menuUnique, CancellationToken cancellationToken = default);
    Task<List<um_menu>> GetActiveMenusAsync(CancellationToken cancellationToken = default);
    Task AddAsync(um_menu entity, CancellationToken cancellationToken = default);
}
