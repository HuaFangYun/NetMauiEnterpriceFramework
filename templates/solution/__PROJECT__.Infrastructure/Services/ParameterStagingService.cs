using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class ParameterStagingService : IParameterStagingService
{
    private readonly __PROJECT__DbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ParameterStagingService(__PROJECT__DbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> StageAsync(string tableName, string type, IReadOnlyDictionary<string, string?> values)
    {
        var id = Guid.NewGuid();
        var now = DateTime.Now;
        var actor = _currentUser.UserId ?? "system";

        await _context.tb_parameter_sytem_temporary.AddAsync(new tb_parameter_sytem_temporary
        {
            id = id,
            table_name = tableName,
            type = type,
            created_by = actor,
            created_date = now,
        });

        foreach (var pair in values)
        {
            await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
            {
                tb_parameter_sytem_temporary_id = id,
                column_name = pair.Key,
                value = pair.Value,
            });
        }

        await _context.SaveChangesAsync();
        return id;
    }

    public Task<bool> HasPendingAsync(string tableName, string keyColumn, string keyValue)
        => (from temporary in _context.tb_parameter_sytem_temporary
            join detail in _context.tb_parameter_sytem_temporary_detail
                on temporary.id equals detail.tb_parameter_sytem_temporary_id
            where temporary.table_name == tableName
                  && (temporary.type == Const.ApprovalType.ADD
                      || temporary.type == Const.ApprovalType.MODIFIED
                      || temporary.type == Const.ApprovalType.DELETE)
                  && detail.column_name == keyColumn
                  && detail.value == keyValue
            select temporary.id).AnyAsync();
}
