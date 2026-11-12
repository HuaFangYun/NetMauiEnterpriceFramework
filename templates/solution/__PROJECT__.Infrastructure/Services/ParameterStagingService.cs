using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Infrastructure.Services;

public class ParameterStagingService : IParameterStagingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ParameterStagingService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> StageAsync(string tableName, string type, IReadOnlyDictionary<string, string?> values)
    {
        var id = Guid.NewGuid();
        var now = DateTime.Now;
        var actor = _currentUser.UserId ?? "system";

        var temporary = new tb_parameter_sytem_temporary
        {
            id = id,
            table_name = tableName,
            type = type,
            created_by = actor,
            created_date = now,
        };

        var details = values.Select(pair => new tb_parameter_sytem_temporary_detail
        {
            tb_parameter_sytem_temporary_id = id,
            column_name = pair.Key,
            value = pair.Value,
        });

        await _unitOfWork.ParameterStaging.AddAsync(temporary, details);
        await _unitOfWork.SaveChangesAsync();
        return id;
    }

    public Task<bool> HasPendingAsync(string tableName, string keyColumn, string keyValue)
        => _unitOfWork.ParameterStaging.HasPendingAsync(tableName, keyColumn, keyValue);
}
