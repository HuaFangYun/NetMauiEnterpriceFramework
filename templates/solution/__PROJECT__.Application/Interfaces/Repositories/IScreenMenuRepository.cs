namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IScreenMenuRepository
{
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> ChildExistsAsync(string childId, CancellationToken cancellationToken = default);
    Task<bool> ImportExistsAsync(string importId, CancellationToken cancellationToken = default);
}
