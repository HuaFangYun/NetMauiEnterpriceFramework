using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IParameterStagingRepository
{
    Task<tb_parameter_sytem_temporary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<tb_parameter_sytem_temporary_detail>> GetDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        tb_parameter_sytem_temporary temporary,
        IEnumerable<tb_parameter_sytem_temporary_detail> details,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingAsync(
        string tableName,
        string keyColumn,
        string keyValue,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingKeyAsync(
        string tableName,
        string keyColumn,
        string keyValue,
        CancellationToken cancellationToken = default);

    void Remove(
        tb_parameter_sytem_temporary temporary,
        IEnumerable<tb_parameter_sytem_temporary_detail> details);
}
