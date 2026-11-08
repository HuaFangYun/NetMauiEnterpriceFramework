using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Parameter;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class ParameterEngineService : IParameterEngineService
{
    private static readonly HashSet<string> AllowedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "rf_default_value",
        "rf_files_upload_type",
        "rf_group_access_report_set",
        "rf_message",
        "tb_parameter_system",
        "tb_parameter_system_detail",
        "tb_report_set",
        "tb_screen_menu",
        "__import__",
    };

    private static readonly string[] AllowedTableNames = AllowedTables.ToArray();

    private readonly __PROJECT__DbContext _context;
    private readonly AuditLogDbContext _audit;
    private readonly ICurrentUserService _currentUser;

    public ParameterEngineService(__PROJECT__DbContext context, AuditLogDbContext audit, ICurrentUserService currentUser)
    {
        _context = context;
        _audit = audit;
        _currentUser = currentUser;
    }

    private string Actor => _currentUser.UserId ?? "system";

    public async Task<List<ParameterCardDto>> GetCardsAsync()
    {
        var pending = await _context.tb_parameter_sytem_temporary
            .Where(e => e.table_name != null)
            .GroupBy(e => e.table_name!)
            .Select(e => new { TableName = e.Key, Count = e.Count() })
            .ToDictionaryAsync(e => e.TableName, e => e.Count);

        var cards = await _context.tb_parameter_system
            .OrderBy(e => e.description)
            .Select(e => new ParameterCardDto
            {
                TableName = e.table_name,
                Description = e.description,
                MakerUrl = e.param_maker_url,
                ApprovalUrl = e.param_approval_url,
                IsMaker = e.is_param_maker == true,
                IsApproval = e.is_param_approval == true,
                IsUpload = e.is_param_upload == true,
                Icon = e.icon,
            })
            .ToListAsync();

        foreach (var card in cards)
        {
            card.PendingCount = pending.GetValueOrDefault(card.TableName);
            card.MakerUrl ??= $"/dashboard/maintenance/parameter-system/maker/{card.TableName}";
            card.ApprovalUrl ??= $"/dashboard/maintenance/parameter-system/approval/{card.TableName}";
        }

        return cards;
    }

    public async Task<List<ParameterFieldDto>> GetFieldsAsync(string tableName)
    {
        tableName = NormalizeTable(tableName);
        var fields = await _context.tb_parameter_system_detail
            .Where(e => e.table_name == tableName)
            .OrderBy(e => e.order_seq)
            .Select(e => new ParameterFieldDto
            {
                FieldName = e.field_name,
                FieldDesc = e.field_desc,
                FieldType = e.field_type,
                FieldReff = e.field_reff,
                IsMandatory = e.is_mandatory == true,
                IsKey = e.field_key == true,
                IsOrderable = e.is_orderable == true,
                OrderSeq = e.order_seq,
                AddClass = e.add_class,
            })
            .ToListAsync();

        return fields.Count > 0 ? fields : await IntrospectFieldsAsync(tableName);
    }

    public async Task<ParameterListResponseDto> GetListAsync(string tableName, DataTableRequestDto request)
    {
        tableName = NormalizeTable(tableName);
        var fields = await GetFieldsAsync(tableName);
        var keyFields = GetKeyFields(fields);
        var searchFields = fields.Where(e => !string.Equals(e.FieldType, "bit", StringComparison.OrdinalIgnoreCase)).Take(8).ToList();

        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);
        var search = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        var sortField = fields.FirstOrDefault(e => string.Equals(e.FieldName, request.SortColumn, StringComparison.OrdinalIgnoreCase))?.FieldName
                        ?? keyFields.First().FieldName;
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        var filters = new List<string>();
        if (search != null)
        {
            filters.Add("(" + string.Join(" OR ", searchFields.Select(e => $"CAST({Quote(e.FieldName)} AS nvarchar(1000)) LIKE @search")) + ")");
        }

        if (await HasColumnAsync(connection, tableName, "is_deleted"))
        {
            filters.Add("[is_deleted] = 0");
        }

        var where = filters.Count > 0 ? " WHERE " + string.Join(" AND ", filters) : string.Empty;

        var countCommand = new SqlCommand($"SELECT COUNT(*) FROM [dbo].{Quote(tableName)}{where}", connection);
        if (search != null)
        {
            countCommand.Parameters.AddWithValue("@search", $"%{search}%");
        }
        var total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(), CultureInfo.InvariantCulture);

        var command = new SqlCommand(
            $"SELECT * FROM [dbo].{Quote(tableName)}{where} ORDER BY {Quote(sortField)} {(descending ? "DESC" : "ASC")} " +
            "OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY", connection);
        command.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("@pageSize", pageSize);
        if (search != null)
        {
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }

        var rows = new List<Dictionary<string, string>>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in fields)
                {
                    var ordinal = reader.GetOrdinal(field.FieldName);
                    row[field.FieldName] = reader.IsDBNull(ordinal) ? string.Empty : FormatValue(reader.GetValue(ordinal), field.FieldType) ?? string.Empty;
                }

                row["__key"] = EncodeKey(keyFields.Select(e => row.GetValueOrDefault(e.FieldName)).ToList());
                rows.Add(row);
            }
        }

        return new ParameterListResponseDto
        {
            Fields = fields,
            Rows = rows,
            TotalCount = total,
            KeyField = string.Join("+", keyFields.Select(e => e.FieldName)),
        };
    }

    public async Task<Dictionary<string, string>?> GetAsync(string tableName, string? key)
    {
        tableName = NormalizeTable(tableName);
        var fields = await GetFieldsAsync(tableName);
        var keyFields = GetKeyFields(fields);
        var keyValues = DecodeKey(key);
        if (keyValues.Count != keyFields.Count)
        {
            return null;
        }

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        var where = string.Join(" AND ", keyFields.Select((field, index) => $"{Quote(field.FieldName)} = @k{index}"));
        if (await HasColumnAsync(connection, tableName, "is_deleted"))
        {
            where += " AND [is_deleted] = 0";
        }

        var command = new SqlCommand($"SELECT TOP 1 * FROM [dbo].{Quote(tableName)} WHERE {where}", connection);
        for (var index = 0; index < keyFields.Count; index++)
        {
            command.Parameters.AddWithValue($"@k{index}", ParseParameter(keyValues[index], keyFields[index].FieldType));
        }

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields)
        {
            var ordinal = reader.GetOrdinal(field.FieldName);
            row[field.FieldName] = reader.IsDBNull(ordinal) ? string.Empty : FormatValue(reader.GetValue(ordinal), field.FieldType) ?? string.Empty;
        }

        row["__key"] = EncodeKey(keyFields.Select(e => row.GetValueOrDefault(e.FieldName)).ToList());
        return row;
    }

    public async Task<BaseGeneralResponse> SaveAsync(ParameterSaveRequestDto request)
    {
        try
        {
            var tableName = NormalizeTable(request.TableName);
            var fields = await GetFieldsAsync(tableName);
            var keyFields = GetKeyFields(fields);
            var keyValues = DecodeKey(request.Key);
            var existing = keyValues.Count == keyFields.Count
                ? await GetAsync(tableName, request.Key)
                : null;
            var isEdit = existing != null;
            var type = isEdit ? BCIConst.ApprovalType.MODIFIED : BCIConst.ApprovalType.ADD;

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < keyFields.Count; index++)
            {
                values[keyFields[index].FieldName] = keyValues.Count == keyFields.Count
                    ? keyValues[index]
                    : request.Values.GetValueOrDefault(keyFields[index].FieldName) ?? string.Empty;
            }

            foreach (var field in fields.Where(e => !e.IsKey))
            {
                values[field.FieldName] = request.Values.GetValueOrDefault(field.FieldName) ?? string.Empty;
            }

            foreach (var field in fields.Where(e => e.IsMandatory))
            {
                if (string.IsNullOrWhiteSpace(values.GetValueOrDefault(field.FieldName)))
                {
                    return Fail($"{field.FieldDesc ?? field.FieldName} is required.");
                }
            }

            var encodedKey = EncodeKey(keyFields.Select(e => values.GetValueOrDefault(e.FieldName)).ToList());
            if (await HasPendingAsync(tableName, encodedKey))
            {
                return Fail("This parameter already has a pending request.");
            }

            var temporary = new tb_parameter_sytem_temporary
            {
                id = Guid.NewGuid(),
                table_name = tableName,
                type = type,
                created_by = Actor,
                created_date = DateTime.Now,
            };
            await _context.tb_parameter_sytem_temporary.AddAsync(temporary);

            foreach (var field in fields)
            {
                await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
                {
                    tb_parameter_sytem_temporary_id = temporary.id,
                    column_name = field.FieldName,
                    value = values.GetValueOrDefault(field.FieldName),
                    value_before = existing?.GetValueOrDefault(field.FieldName),
                });
            }

            await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
            {
                tb_parameter_sytem_temporary_id = temporary.id,
                column_name = "__key",
                value = encodedKey,
            });

            await _context.SaveChangesAsync();
            return Success("Parameter request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string tableName, string? key)
    {
        try
        {
            tableName = NormalizeTable(tableName);
            var fields = await GetFieldsAsync(tableName);
            var keyFields = GetKeyFields(fields);
            var keyValues = DecodeKey(key);
            var existing = await GetAsync(tableName, key);
            if (existing == null || keyValues.Count != keyFields.Count)
            {
                return Fail("Parameter not found.");
            }

            if (await HasPendingAsync(tableName, key ?? string.Empty))
            {
                return Fail("This parameter already has a pending request.");
            }

            var temporary = new tb_parameter_sytem_temporary
            {
                id = Guid.NewGuid(),
                table_name = tableName,
                type = BCIConst.ApprovalType.DELETE,
                created_by = Actor,
                created_date = DateTime.Now,
            };
            await _context.tb_parameter_sytem_temporary.AddAsync(temporary);

            for (var index = 0; index < keyFields.Count; index++)
            {
                await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
                {
                    tb_parameter_sytem_temporary_id = temporary.id,
                    column_name = keyFields[index].FieldName,
                    value = keyValues[index],
                });
            }

            await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
            {
                tb_parameter_sytem_temporary_id = temporary.id,
                column_name = "__key",
                value = key,
            });

            await _context.SaveChangesAsync();
            return Success("Parameter deletion request sent for approval.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<GeneralListResponseDto<ParameterPendingListItemDto>> GetPendingAsync(ParameterPendingRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = _context.tb_parameter_sytem_temporary.Where(e =>
            e.table_name != null
            && AllowedTableNames.Contains(e.table_name)
            && (e.type == BCIConst.ApprovalType.ADD
                || e.type == BCIConst.ApprovalType.MODIFIED
                || e.type == BCIConst.ApprovalType.DELETE));

        if (!string.IsNullOrWhiteSpace(request.TableName))
        {
            var tableName = NormalizeTable(request.TableName);
            query = query.Where(e => e.table_name == tableName);
        }

        var rows = await query.OrderByDescending(e => e.created_date).ToListAsync();
        var details = await LoadDetailsAsync(rows.Select(e => e.id));
        var requesters = await _context.um_user_app
            .Where(e => e.user_id != null)
            .ToDictionaryAsync(e => e.user_id, e => e.full_name);

        var list = rows.Select(row =>
        {
            var values = details.GetValueOrDefault(row.id) ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var key = Get(values, "__key");
            var isImport = string.Equals(row.table_name, "__import__", StringComparison.OrdinalIgnoreCase);
            var keyValue = isImport
                ? Get(values, "__file_name") ?? key
                : string.Join(" / ", DecodeKey(key));
            var summary = isImport
                ? Get(values, "__summary")
                : string.Join(", ", values
                    .Where(e => !e.Key.Equals("__key", StringComparison.OrdinalIgnoreCase) && !IsAuditDetail(e.Key))
                    .Take(3)
                    .Select(e => $"{e.Key}={e.Value}"));
            return new ParameterPendingListItemDto
            {
                Id = row.id,
                TableName = isImport ? Get(values, "__target") ?? "import" : row.table_name ?? string.Empty,
                Type = isImport ? "Import" : TypeLabel(row.type),
                KeyValue = keyValue,
                Summary = summary,
                RequesterFullName = requesters.GetValueOrDefault(row.created_by ?? string.Empty) ?? row.created_by,
                CreatedDate = row.created_date,
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            list = list.Where(e =>
                e.TableName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (e.KeyValue?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (e.Summary?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (e.RequesterFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        list = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "tablename" => descending ? list.OrderByDescending(e => e.TableName).ToList() : list.OrderBy(e => e.TableName).ToList(),
            "createddate" => descending ? list.OrderByDescending(e => e.CreatedDate).ToList() : list.OrderBy(e => e.CreatedDate).ToList(),
            _ => descending ? list.OrderByDescending(e => e.TableName).ToList() : list.OrderBy(e => e.TableName).ToList(),
        };

        return new GeneralListResponseDto<ParameterPendingListItemDto>
        {
            Data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
        };
    }

    public async Task<BaseGeneralResponse> ApproveAsync(Guid id)
    {
        var temporary = await _context.tb_parameter_sytem_temporary.FirstOrDefaultAsync(e => e.id == id);
        if (temporary == null || string.IsNullOrWhiteSpace(temporary.table_name))
        {
            return Fail("Pending request not found.");
        }

        var tableName = NormalizeTable(temporary.table_name);
        var details = await LoadDetailsAsync(id);
        if (tableName == "__import__" && details.ContainsKey("__import"))
        {
            return await ApproveImportAsync(temporary, details);
        }

        if (tableName == "tb_screen_menu" && details.ContainsKey("screen.id"))
        {
            return await ApproveScreenMenuAsync(temporary, details);
        }

        var fields = await GetFieldsAsync(tableName);
        var keyFields = GetKeyFields(fields);
        var keyValues = keyFields.Select(e => Get(details, e.FieldName)).ToList();
        if (keyValues.Any(string.IsNullOrWhiteSpace))
        {
            return Fail("Key payload is missing.");
        }

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            var columns = await GetTableColumnsAsync(connection, transaction, tableName);
            var now = DateTime.Now;

            if (temporary.type == BCIConst.ApprovalType.DELETE)
            {
                var where = string.Join(" AND ", keyFields.Select((field, index) => $"{Quote(field.FieldName)} = @k{index}"));
                var command = new SqlCommand { Connection = connection, Transaction = transaction };
                if (columns.Contains("is_deleted"))
                {
                    var sets = new List<string> { "[is_deleted] = 1" };
                    if (columns.Contains("is_active")) sets.Add("[is_active] = 0");
                    if (columns.Contains("status_approval")) sets.Add("[status_approval] = @approved");
                    if (columns.Contains("deleted_by")) sets.Add("[deleted_by] = @actor");
                    if (columns.Contains("deleted_date")) sets.Add("[deleted_date] = @now");
                    if (columns.Contains("approved_by")) sets.Add("[approved_by] = @actor");
                    if (columns.Contains("approved_date")) sets.Add("[approved_date] = @now");
                    command.CommandText = $"UPDATE [dbo].{Quote(tableName)} SET {string.Join(", ", sets)} WHERE {where}";
                    command.Parameters.AddWithValue("@approved", BCIConst.StatusApproval.APPROVED);
                    command.Parameters.AddWithValue("@actor", Actor);
                    command.Parameters.AddWithValue("@now", now);
                }
                else
                {
                    command.CommandText = $"DELETE FROM [dbo].{Quote(tableName)} WHERE {where}";
                }

                for (var index = 0; index < keyFields.Count; index++)
                {
                    command.Parameters.AddWithValue($"@k{index}", ParseParameter(keyValues[index], keyFields[index].FieldType));
                }

                if (await command.ExecuteNonQueryAsync() == 0)
                {
                    return Fail("Parameter not found.");
                }
            }
            else
            {
                var values = fields.ToDictionary(e => e.FieldName, e => Get(details, e.FieldName), StringComparer.OrdinalIgnoreCase);
                if (temporary.type == BCIConst.ApprovalType.ADD)
                {
                    var insertColumns = new List<string>();
                    var insertParameters = new List<string>();
                    var command = new SqlCommand { Connection = connection, Transaction = transaction };

                    foreach (var field in fields.Where(e => columns.Contains(e.FieldName)))
                    {
                        insertColumns.Add(Quote(field.FieldName));
                        insertParameters.Add($"@p{insertColumns.Count - 1}");
                        var parameter = command.Parameters.AddWithValue($"@p{insertColumns.Count - 1}", ParseParameter(values.GetValueOrDefault(field.FieldName), field.FieldType));
                        parameter.IsNullable = true;
                    }

                    AddAuditInsertColumns(columns, command, insertColumns, insertParameters, now);
                    command.CommandText = $"INSERT INTO [dbo].{Quote(tableName)} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertParameters)})";
                    await command.ExecuteNonQueryAsync();
                }
                else
                {
                    var sets = new List<string>();
                    var command = new SqlCommand { Connection = connection, Transaction = transaction };
                    foreach (var field in fields.Where(e => !e.IsKey && columns.Contains(e.FieldName)))
                    {
                        sets.Add($"{Quote(field.FieldName)} = @p{sets.Count}");
                        var parameter = command.Parameters.AddWithValue($"@p{sets.Count - 1}", ParseParameter(values.GetValueOrDefault(field.FieldName), field.FieldType));
                        parameter.IsNullable = true;
                    }

                    AddAuditUpdateColumns(columns, command, sets, now);
                    var where = string.Join(" AND ", keyFields.Select((field, index) => $"{Quote(field.FieldName)} = @k{index}"));
                    command.CommandText = $"UPDATE [dbo].{Quote(tableName)} SET {string.Join(", ", sets)} WHERE {where}";
                    for (var index = 0; index < keyFields.Count; index++)
                    {
                        command.Parameters.AddWithValue($"@k{index}", ParseParameter(keyValues[index], keyFields[index].FieldType));
                    }

                    if (await command.ExecuteNonQueryAsync() == 0)
                    {
                        return Fail("Parameter not found.");
                    }
                }
            }

            await transaction.CommitAsync();

            await RemoveTemporaryAsync(temporary);
            await _context.SaveChangesAsync();
            await AuditAsync("ParameterApproval", "APPROVE", $"{tableName} approved ({TypeLabel(temporary.type)}).");
            return Success("Parameter approval applied.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    public async Task<BaseGeneralResponse> RejectAsync(Guid id)
    {
        var temporary = await _context.tb_parameter_sytem_temporary.FirstOrDefaultAsync(e => e.id == id);
        if (temporary == null)
        {
            return Fail("Pending request not found.");
        }

        await RemoveTemporaryAsync(temporary);
        await _context.SaveChangesAsync();
        await AuditAsync("ParameterApproval", "REJECT", $"{temporary.table_name} rejected ({TypeLabel(temporary.type)}).");
        return Success("Parameter request rejected.");
    }

    private async Task<List<ParameterFieldDto>> IntrospectFieldsAsync(string tableName)
    {
        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        var command = new SqlCommand(
            "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table ORDER BY ORDINAL_POSITION", connection);
        command.Parameters.AddWithValue("@table", tableName);

        var fields = new List<ParameterFieldDto>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var name = reader.GetString(0);
            var type = MapSqlType(reader.GetString(1));
            fields.Add(new ParameterFieldDto
            {
                FieldName = name,
                FieldDesc = name,
                FieldType = type,
                IsKey = fields.Count == 0,
                OrderSeq = fields.Count + 1,
            });
        }

        return fields;
    }

    private async Task<HashSet<string>> GetTableColumnsAsync(SqlConnection connection, SqlTransaction? transaction, string tableName)
    {        var command = new SqlCommand(
            "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table", connection, transaction);
        command.Parameters.AddWithValue("@table", tableName);
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private static async Task<bool> HasColumnAsync(SqlConnection connection, string tableName, string columnName)
    {
        var command = new SqlCommand(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND COLUMN_NAME = @column", connection);
        command.Parameters.AddWithValue("@table", tableName);
        command.Parameters.AddWithValue("@column", columnName);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture) > 0;
    }

    private void AddAuditInsertColumns(HashSet<string> columns, SqlCommand command, List<string> insertColumns, List<string> insertParameters, DateTime now)    {
        AddInsertColumn(columns, command, insertColumns, insertParameters, "created_by", Actor);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "created_date", now);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "status_approval", BCIConst.StatusApproval.APPROVED);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "is_active", true);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "is_deleted", false);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "is_locked", false);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "is_default", false);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "approved_by", Actor);
        AddInsertColumn(columns, command, insertColumns, insertParameters, "approved_date", now);
    }

    private static void AddInsertColumn(HashSet<string> columns, SqlCommand command, List<string> insertColumns, List<string> insertParameters, string column, object value)
    {
        if (!columns.Contains(column) || insertColumns.Any(e => e.Trim('[', ']').Equals(column, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        insertColumns.Add($"[{column}]");
        insertParameters.Add($"@a{insertParameters.Count}");
        command.Parameters.AddWithValue($"@a{insertParameters.Count - 1}", value);
    }

    private void AddAuditUpdateColumns(HashSet<string> columns, SqlCommand command, List<string> sets, DateTime now)
    {
        AddUpdateColumn(columns, command, sets, "modified_by", Actor);
        AddUpdateColumn(columns, command, sets, "modified_date", now);
        AddUpdateColumn(columns, command, sets, "status_approval", BCIConst.StatusApproval.APPROVED);
        AddUpdateColumn(columns, command, sets, "approved_by", Actor);
        AddUpdateColumn(columns, command, sets, "approved_date", now);
    }

    private static void AddUpdateColumn(HashSet<string> columns, SqlCommand command, List<string> sets, string column, object value)
    {
        if (!columns.Contains(column) || sets.Any(e => e.StartsWith($"[{column}]", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        sets.Add($"[{column}] = @a{sets.Count}");
        command.Parameters.AddWithValue($"@a{sets.Count - 1}", value);
    }

    private async Task<BaseGeneralResponse> ApproveImportAsync(
        tb_parameter_sytem_temporary temporary,
        Dictionary<string, string?> details)
    {
        try
        {
            var targetTable = Get(details, "__target")?.Trim() ?? string.Empty;
            if (!Regex.IsMatch(targetTable, "^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                return Fail("Import target table is invalid.");
            }

            var columns = JsonSerializer.Deserialize<List<string>>(Get(details, "__columns") ?? "[]") ?? new List<string>();
            var rows = JsonSerializer.Deserialize<List<List<string?>>>(Get(details, "__rows") ?? "[]") ?? new List<List<string?>>();
            if (columns.Count == 0 || rows.Count == 0)
            {
                return Fail("Import staging payload is empty.");
            }

            if (columns.Any(e => !Regex.IsMatch(e, "^[A-Za-z_][A-Za-z0-9_]*$")))
            {
                return Fail("Import staging contains an invalid column.");
            }

            if (rows.Any(row => row.Count != columns.Count))
            {
                return Fail("Import staging row/column count mismatch.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            var knownColumns = await GetTableColumnsAsync(connection, null!, targetTable);
            var unknownColumn = columns.FirstOrDefault(e => !knownColumns.Contains(e));
            if (unknownColumn != null)
            {
                return Fail($"Target column '{unknownColumn}' was not found in {targetTable}.");
            }

            var columnList = string.Join(", ", columns.Select(e => $"[{e}]"));
            var parameterList = string.Join(", ", columns.Select((_, index) => $"@p{index}"));
            var insertSql = $"INSERT INTO [dbo].[{targetTable}] ({columnList}) VALUES ({parameterList})";

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = insertSql;
                for (var index = 0; index < columns.Count; index++)
                {
                    command.Parameters.Add(new SqlParameter($"@p{index}", SqlDbType.NVarChar, -1));
                }

                foreach (var row in rows)
                {
                    for (var index = 0; index < row.Count; index++)
                    {
                        command.Parameters[index].Value = (object?)row[index] ?? DBNull.Value;
                    }

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await RemoveTemporaryAsync(temporary);
            await _context.SaveChangesAsync();
            await AuditAsync("ParameterApproval", "APPROVE", $"Import {rows.Count} row(s) into {targetTable} approved.");
            return Success($"{rows.Count} row(s) imported into {targetTable}.");
        }
        catch (Exception ex)
        {
            return Fail($"Import approval failed: {ex.Message}");
        }
    }

    private async Task<BaseGeneralResponse> ApproveScreenMenuAsync(
        tb_parameter_sytem_temporary temporary,
        Dictionary<string, string?> details)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var now = DateTime.Now;
            var actor = Actor;
            var screenId = Get(details, "screen.id");
            if (string.IsNullOrWhiteSpace(screenId))
            {
                return Fail("Screen id is missing.");
            }

            var childId = Get(details, "child.screen_menu_child_id");
            if (!string.IsNullOrWhiteSpace(childId))
            {
                var child = await _context.tb_screen_menu_child
                    .FirstOrDefaultAsync(e => e.screen_menu_child_id == childId);
                if (child == null)
                {
                    child = new tb_screen_menu_child
                    {
                        screen_menu_child_id = childId,
                        createdBy = temporary.created_by,
                        createdDate = temporary.created_date,
                    };
                    await _context.tb_screen_menu_child.AddAsync(child);
                }

                child.description = Get(details, "child.description");
                child.type = Get(details, "child.type");
                child.url_action = Get(details, "child.url_action");
                child.url_action_param = Get(details, "child.url_action_param");
                child.modal_field = Get(details, "child.modal_field");
                child.modal_detail_field_query = Get(details, "child.modal_detail_field_query");
                child.modal_detail_field_query_param = Get(details, "child.modal_detail_field_query_param");
                child.modal_save_process_query = Get(details, "child.modal_save_process_query");
                child.modal_save_process_query_param = Get(details, "child.modal_save_process_query_param");
                child.isActive = ParseBool(Get(details, "child.is_active"));
                child.isDeleted = false;
                child.modifiedBy = actor;
                child.modifiedDate = now;
            }

            var importId = Get(details, "import.screen_menu_import_excel_process_id");
            if (!string.IsNullOrWhiteSpace(importId))
            {
                var import = await _context.tb_screen_menu_import_excel_process
                    .FirstOrDefaultAsync(e => e.screen_menu_import_excel_process_id == importId);
                if (import == null)
                {
                    import = new tb_screen_menu_import_excel_process
                    {
                        screen_menu_import_excel_process_id = importId,
                        createdBy = temporary.created_by,
                        createdDate = temporary.created_date,
                    };
                    await _context.tb_screen_menu_import_excel_process.AddAsync(import);
                }

                import.description = Get(details, "import.description");
                import.table_name = Get(details, "import.table_name");
                import.field_input = Get(details, "import.field_input");
                import.with_approval = ParseBool(Get(details, "import.with_approval"));
                import.isActive = ParseBool(Get(details, "import.is_active"));
                import.isDeleted = false;
                import.modifiedBy = actor;
                import.modifiedDate = now;
            }

            var screen = await _context.tb_screen_menu.FirstOrDefaultAsync(e => e.id == screenId);
            if (screen == null)
            {
                screen = new tb_screen_menu
                {
                    id = screenId,
                    createdBy = temporary.created_by,
                    createdDate = temporary.created_date,
                };
                await _context.tb_screen_menu.AddAsync(screen);
            }

            screen.description = Get(details, "screen.description");
            screen.has_button_load = ParseBool(Get(details, "screen.has_button_load"));
            screen.has_button_process = ParseBool(Get(details, "screen.has_button_process"));
            screen.has_button_generate_txt = ParseBool(Get(details, "screen.has_button_generate_txt"));
            screen.has_button_export_excel = ParseBool(Get(details, "screen.has_button_export_excel"));
            screen.has_button_add = ParseBool(Get(details, "screen.has_button_add"));
            screen.has_button_delete = ParseBool(Get(details, "screen.has_button_delete"));
            screen.has_action_row = ParseBool(Get(details, "screen.has_action_row"));
            screen.has_button_import_excel = ParseBool(Get(details, "screen.has_button_import_excel"));
            screen.field_filter = Get(details, "screen.field_filter");
            screen.query_load = Get(details, "screen.query_load");
            screen.query_load_param = Get(details, "screen.query_load_param");
            screen.query_process = Get(details, "screen.query_process");
            screen.query_process_param = Get(details, "screen.query_process_param");
            screen.query_export_txt = Get(details, "screen.query_export_txt");
            screen.query_export_txt_param = Get(details, "screen.query_export_txt_param");
            screen.query_delete = Get(details, "screen.query_delete");
            screen.query_delete_param = Get(details, "screen.query_delete_param");
            screen.screen_menu_child_id = childId;
            screen.screen_menu_import_excel_process_id = importId;
            screen.isActive = ParseBool(Get(details, "screen.is_active"));
            screen.isDeleted = false;
            screen.statusApproval = BCIConst.StatusApproval.APPROVED;
            screen.approvedBy = actor;
            screen.approvedDate = now;
            screen.modifiedBy = temporary.created_by;
            screen.modifiedDate = temporary.created_date;

            await RemoveTemporaryAsync(temporary);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await AuditAsync("ParameterApproval", "APPROVE", $"Screen menu {screenId} approved with child/import.");
            return Success("Screen menu approval applied.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    private static bool ParseBool(string? value)
        => value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private async Task<bool> HasPendingAsync(string tableName, string key)
        => await (from temporary in _context.tb_parameter_sytem_temporary
                  join detail in _context.tb_parameter_sytem_temporary_detail
                      on temporary.id equals detail.tb_parameter_sytem_temporary_id
                  where temporary.table_name == tableName
                        && (temporary.type == BCIConst.ApprovalType.ADD
                            || temporary.type == BCIConst.ApprovalType.MODIFIED
                            || temporary.type == BCIConst.ApprovalType.DELETE)
                        && detail.column_name == "__key"
                        && detail.value == key
                  select temporary.id).AnyAsync();

    private async Task<Dictionary<Guid, Dictionary<string, string?>>> LoadDetailsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        var rows = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => idList.Contains(e.tb_parameter_sytem_temporary_id))
            .ToListAsync();

        return rows
            .GroupBy(e => e.tb_parameter_sytem_temporary_id)
            .ToDictionary(
                e => e.Key,
                e => e.ToDictionary(x => x.column_name, x => x.value, StringComparer.OrdinalIgnoreCase));
    }

    private async Task<Dictionary<string, string?>> LoadDetailsAsync(Guid id)
    {
        var rows = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => e.tb_parameter_sytem_temporary_id == id)
            .ToListAsync();
        return rows.ToDictionary(e => e.column_name, e => e.value, StringComparer.OrdinalIgnoreCase);
    }

    private async Task RemoveTemporaryAsync(tb_parameter_sytem_temporary temporary)
    {
        var details = await _context.tb_parameter_sytem_temporary_detail
            .Where(e => e.tb_parameter_sytem_temporary_id == temporary.id)
            .ToListAsync();
        _context.tb_parameter_sytem_temporary_detail.RemoveRange(details);
        _context.tb_parameter_sytem_temporary.Remove(temporary);
    }

    private async Task AuditAsync(string entity, string action, string message)
    {
        try
        {
            _audit.tb_log.Add(new tb_log
            {
                entity = entity,
                log_type = action,
                log_msg = message.Length > 4000 ? message[..4000] : message,
                created_by = Actor,
                created_date = DateTime.Now,
            });
            await _audit.SaveChangesAsync();
        }
        catch
        {
            // Auditing must not block an approved transaction.
        }
    }

    private static string NormalizeTable(string tableName)
    {
        tableName = (tableName ?? string.Empty).Trim();
        if (!AllowedTables.Contains(tableName))
        {
            throw new InvalidOperationException("Table is not registered as a parameter.");
        }

        return tableName;
    }

    private static List<ParameterFieldDto> GetKeyFields(List<ParameterFieldDto> fields)
    {
        var keys = fields.Where(e => e.IsKey).ToList();
        return keys.Count > 0 ? keys : new List<ParameterFieldDto> { fields[0] };
    }

    private static string Quote(string identifier)
        => "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";

    private static object ParseParameter(string? value, string? type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DBNull.Value;
        }

        return type?.ToLowerInvariant() switch
        {
            "bit" => value == "1" || bool.TryParse(value, out var bit) && bit,
            "number" or "int" or "decimal" => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number)
                ? number
                : DBNull.Value,
            "date" or "datetime" => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : DBNull.Value,
            _ => value,
        };
    }

    private static string? FormatValue(object value, string? type) => value switch
    {
        bool boolean => boolean ? "true" : "false",
        DateTime dateTime when string.Equals(type, "date", StringComparison.OrdinalIgnoreCase) => dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture),
    };

    private static string MapSqlType(string type) => type.ToLowerInvariant() switch
    {
        "bit" => "bit",
        "int" or "bigint" or "smallint" or "tinyint" or "decimal" or "numeric" or "money" or "float" or "real" => "number",
        "date" or "datetime" or "datetime2" or "smalldatetime" or "datetimeoffset" or "time" => "date",
        _ => "text",
    };

    private static string EncodeKey(IEnumerable<string?> values)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join("\u001f", values.Select(e => e ?? string.Empty))));

    private static List<string> DecodeKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new List<string>();
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(key)).Split('\u001f').ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string? Get(Dictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) ? value : null;

    private static bool IsAuditDetail(string column) => column is
        "approval_type_id" or "approved_by" or "approved_date"
        or "created_by" or "created_date" or "modified_by" or "modified_date"
        or "deleted_by" or "deleted_date" or "status_approval";

    private static string TypeLabel(string? type) => type switch
    {
        BCIConst.ApprovalType.ADD => "Add",
        BCIConst.ApprovalType.MODIFIED => "Modify",
        BCIConst.ApprovalType.DELETE => "Delete",
        _ => type ?? string.Empty,
    };

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = BCIConst.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = BCIConst.ERROR_CODE,
        Message = message,
    };
}
