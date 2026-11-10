using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;
using __PROJECT__.Infrastructure.Dynamic;

namespace __PROJECT__.Infrastructure.Services;

public class DynamicEngineService : IDynamicEngineService
{
    private readonly __PROJECT__DbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;

    public DynamicEngineService(
        __PROJECT__DbContext context,
        ICurrentUserService currentUser,
        IErrorLogService errorLog)
    {
        _context = context;
        _currentUser = currentUser;
        _errorLog = errorLog;
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("DynamicEngine", "ERROR", exception);

    public async Task<DynamicScreenDto?> GetScreenAsync(string screenId)
    {
        if (!await CanAccessScreenAsync(screenId))
        {
            return null;
        }

        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId
                                      && e.isActive != false
                                      && e.isDeleted != true
                                      && e.statusApproval == Const.StatusApproval.APPROVED);
        if (screen == null)
        {
            return null;
        }

        return new DynamicScreenDto
        {
            ScreenId = screen.id,
            Description = screen.description,
            Fields = FieldFilterParser.Parse(screen.field_filter),
            HasButtonLoad = screen.has_button_load == true,
            HasButtonProcess = screen.has_button_process == true,
            HasButtonGenerateTxt = screen.has_button_generate_txt == true,
            HasButtonExportExcel = screen.has_button_export_excel == true,
            HasButtonAdd = screen.has_button_add == true,
            HasButtonDelete = screen.has_button_delete == true,
            HasActionRow = screen.has_action_row == true,
            HasButtonImportExcel = screen.has_button_import_excel == true,
            ScreenMenuChildId = screen.screen_menu_child_id,
            Child = await GetChildAsync(screen.screen_menu_child_id),
        };
    }

    private async Task<DynamicChildDto?> GetChildAsync(string? childId)
    {
        if (string.IsNullOrWhiteSpace(childId))
        {
            return null;
        }

        var child = await _context.tb_screen_menu_child
            .FirstOrDefaultAsync(e => e.screen_menu_child_id == childId && e.isDeleted != true);
        return child == null
            ? null
            : new DynamicChildDto
            {
                ScreenMenuChildId = child.screen_menu_child_id,
                Type = child.type,
                UrlAction = child.url_action,
                UrlActionParam = child.url_action_param,
                ModalFields = FieldFilterParser.Parse(child.modal_field),
                HasModalDetail = !string.IsNullOrWhiteSpace(child.modal_detail_field_query),
                HasModalSave = !string.IsNullOrWhiteSpace(child.modal_save_process_query),
            };
    }

    public async Task<Dictionary<string, object?>?> GetChildDetailAsync(string screenId, string key)
    {
        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
            ?? throw new InvalidOperationException("Screen menu not found.");
        var child = await _context.tb_screen_menu_child
            .FirstOrDefaultAsync(e => e.screen_menu_child_id == screen.screen_menu_child_id && e.isDeleted != true)
            ?? throw new InvalidOperationException("Child screen not found.");
        if (string.IsNullOrWhiteSpace(child.modal_detail_field_query))
        {
            return null;
        }

        var fields = FieldFilterParser.Parse(child.modal_field);
        var tokens = FieldFilterParser.ParseParams(child.modal_detail_field_query_param);
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (tokens.Count > 0)
        {
            values[tokens[0].Tag] = key;
        }
        else if (fields.Count > 0)
        {
            values[fields[0].Id] = key;
        }

        var rows = await ExecuteRowsAsync(child.modal_detail_field_query, child.modal_detail_field_query_param, fields, values);
        return rows.FirstOrDefault();
    }

    public async Task<BaseGeneralResponse> SaveChildAsync(string screenId, DynamicMutationRequestDto request)
    {
        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
            ?? throw new InvalidOperationException("Screen menu not found.");
        var child = await _context.tb_screen_menu_child
            .FirstOrDefaultAsync(e => e.screen_menu_child_id == screen.screen_menu_child_id && e.isDeleted != true)
            ?? throw new InvalidOperationException("Child screen not found.");
        if (string.IsNullOrWhiteSpace(child.modal_save_process_query))
        {
            return Fail("Child save query is not configured.");
        }

        return await ExecuteMutationAsync(
            child.modal_save_process_query,
            child.modal_save_process_query_param,
            FieldFilterParser.Parse(child.modal_field),
            request.Values);
    }

    public async Task<BaseGeneralResponse> ProcessAsync(string screenId, DynamicMutationRequestDto request)
    {
        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
            ?? throw new InvalidOperationException("Screen menu not found.");
        if (string.IsNullOrWhiteSpace(screen.query_process))
        {
            return Fail("Process query is not configured.");
        }

        return await ExecuteMutationAsync(
            screen.query_process,
            screen.query_process_param,
            FieldFilterParser.Parse(screen.field_filter),
            request.Values);
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string screenId, DynamicMutationRequestDto request)
    {
        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
            ?? throw new InvalidOperationException("Screen menu not found.");
        if (string.IsNullOrWhiteSpace(screen.query_delete))
        {
            return Fail("Delete query is not configured.");
        }

        return await ExecuteMutationAsync(
            screen.query_delete,
            screen.query_delete_param,
            FieldFilterParser.Parse(screen.field_filter),
            request.Values);
    }

    public async Task<BaseGeneralResponse> ImportExcelAsync(string screenId, Stream stream, string fileName)
    {
        try
        {
            var screen = await _context.tb_screen_menu
                .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
                ?? throw new InvalidOperationException("Screen menu not found.");
            if (screen.has_button_import_excel != true)
            {
                return Fail("Import Excel is not enabled for this screen.");
            }

            var import = await _context.tb_screen_menu_import_excel_process
                .FirstOrDefaultAsync(e => e.screen_menu_import_excel_process_id == screen.screen_menu_import_excel_process_id
                                          && e.isDeleted != true
                                          && e.isActive != false)
                ?? throw new InvalidOperationException("Import process metadata not found.");

            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Fail("Only .xlsx files are supported.");
            }

            var tableName = import.table_name?.Trim() ?? string.Empty;
            if (!Regex.IsMatch(tableName, "^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                return Fail("Import table name is invalid.");
            }

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Excel workbook has no worksheet.");
            var range = sheet.RangeUsed()
                ?? throw new InvalidOperationException("Excel worksheet is empty.");
            var headerRow = range.FirstRow();
            var headerCells = headerRow.CellsUsed()
                .Where(e => !string.IsNullOrWhiteSpace(e.GetString()))
                .GroupBy(e => e.GetString().Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(e => e.Key, e => e.First().Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

            var mappings = new List<ImportColumnMapping>();
            var fieldInput = (import.field_input ?? string.Empty)
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (fieldInput.Length == 0)
            {
                fieldInput = headerCells.Keys.ToArray();
            }

            foreach (var entry in fieldInput)
            {
                var separator = entry.IndexOf('=');
                var header = (separator >= 0 ? entry[..separator] : entry).Trim();
                var column = (separator >= 0 ? entry[(separator + 1)..] : entry).Trim();
                if (!Regex.IsMatch(column, "^[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    return Fail($"Import column '{column}' is invalid.");
                }

                if (!headerCells.TryGetValue(header, out var columnNumber))
                {
                    return Fail($"Excel column '{header}' was not found.");
                }

                mappings.Add(new ImportColumnMapping(column, columnNumber));
            }

            if (mappings.Count == 0)
            {
                return Fail("Import field mapping is empty.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            var knownColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var columnsCommand = connection.CreateCommand())
            {
                columnsCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table";
                columnsCommand.Parameters.AddWithValue("@table", tableName);
                await using var reader = await columnsCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    knownColumns.Add(reader.GetString(0));
                }
            }

            var unknownColumn = mappings.FirstOrDefault(e => !knownColumns.Contains(e.Column));
            if (unknownColumn != null)
            {
                return Fail($"Target column '{unknownColumn.Column}' was not found in {tableName}.");
            }

            var rows = new List<List<string?>>();
            var lastRow = range.LastRow().RowNumber();
            for (var rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRow; rowNumber++)
            {
                var row = sheet.Row(rowNumber);
                var values = mappings
                    .Select(e => row.Cell(e.ColumnNumber).IsEmpty() ? null : row.Cell(e.ColumnNumber).GetString())
                    .ToList();
                if (values.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                rows.Add(values);
                if (rows.Count > 5000)
                {
                    return Fail("Import exceeds the 5000 row limit.");
                }
            }

            if (rows.Count == 0)
            {
                return Fail("Excel contains no data rows.");
            }

            if (import.with_approval == true)
            {
                var temporary = new tb_parameter_sytem_temporary
                {
                    id = Guid.NewGuid(),
                    table_name = "__import__",
                    type = Const.ApprovalType.ADD,
                    created_by = _currentUser.UserId ?? "system",
                    created_date = DateTime.Now,
                };
                await _context.tb_parameter_sytem_temporary.AddAsync(temporary);

                var details = new Dictionary<string, string?>
                {
                    ["__import"] = "1",
                    ["__key"] = fileName,
                    ["__summary"] = $"{rows.Count} row(s) → {tableName}",
                    ["__target"] = tableName,
                    ["__columns"] = JsonSerializer.Serialize(mappings.Select(e => e.Column)),
                    ["__rows"] = JsonSerializer.Serialize(rows),
                    ["__file_name"] = fileName,
                    ["__import_id"] = import.screen_menu_import_excel_process_id,
                };
                foreach (var detail in details)
                {
                    await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
                    {
                        tb_parameter_sytem_temporary_id = temporary.id,
                        column_name = detail.Key,
                        value = detail.Value,
                    });
                }

                await _context.SaveChangesAsync();
                return Success($"{rows.Count} row(s) sent for approval.");
            }

            var columnList = string.Join(", ", mappings.Select(e => $"[{e.Column}]"));
            var parameterList = string.Join(", ", mappings.Select((_, index) => $"@p{index}"));
            var insertSql = $"INSERT INTO [dbo].[{tableName}] ({columnList}) VALUES ({parameterList})";

            var imported = 0;
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = insertSql;
                for (var index = 0; index < mappings.Count; index++)
                {
                    command.Parameters.Add(new SqlParameter($"@p{index}", SqlDbType.NVarChar, -1));
                }

                foreach (var values in rows)
                {
                    for (var index = 0; index < values.Count; index++)
                    {
                        command.Parameters[index].Value = (object?)values[index] ?? DBNull.Value;
                    }

                    await command.ExecuteNonQueryAsync();
                    imported++;
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Success($"{imported} row(s) imported from {fileName}.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail($"Import failed: {ex.Message}");
        }
    }

    private sealed record ImportColumnMapping(string Column, int ColumnNumber);

    public async Task<DynamicExportResultDto> ExportTxtAsync(string screenId, DynamicLoadRequestDto request)
    {
        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
            ?? throw new InvalidOperationException("Screen menu not found.");

        var query = !string.IsNullOrWhiteSpace(screen.query_export_txt) ? screen.query_export_txt : screen.query_load;
        var queryParam = !string.IsNullOrWhiteSpace(screen.query_export_txt) ? screen.query_export_txt_param : screen.query_load_param;
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new InvalidOperationException("Screen has no export or load query.");
        }

        var fields = FieldFilterParser.Parse(screen.field_filter);
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields)
        {
            values[field.Id] = request.Filters.GetValueOrDefault(field.Id);
        }
        values["searchTerm"] = request.SearchTerm;
        values["sortColumn"] = request.SortColumn;
        values["sortDirection"] = request.SortDirection;
        values["sessionuserid"] = _currentUser.UserId;

        var rows = await ExecuteRowsAsync(query, queryParam, fields, values);
        var columns = rows.FirstOrDefault()?.Keys.ToList() ?? new List<string>();
        var builder = new StringBuilder();
        builder.AppendLine(string.Join("|", columns));
        foreach (var row in rows)
        {
            builder.AppendLine(string.Join("|", columns.Select(column =>
                (Convert.ToString(row.GetValueOrDefault(column), CultureInfo.InvariantCulture) ?? string.Empty)
                    .Replace("|", "/", StringComparison.Ordinal)
                    .Replace("\r", " ", StringComparison.Ordinal)
                    .Replace("\n", " ", StringComparison.Ordinal))));
        }

        return new DynamicExportResultDto
        {
            FileName = $"{screenId}_{DateTime.Now:yyyyMMddHHmmss}.txt",
            Content = builder.ToString(),
        };
    }

    public Task<bool> CanAccessScreenAsync(string screenId)
        => CanAccessScreenAsync(screenId, _currentUser.UserId);

    private async Task<bool> CanAccessScreenAsync(string screenId, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var groupId = await _context.um_user_group
            .Where(e => e.user_id == userId && e.isDeleted != true)
            .Select(e => e.group_id)
            .FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return false;
        }

        if (groupId == Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE)
        {
            return true;
        }

        return await (from groupMenu in _context.um_group_menu
                      join menu in _context.um_menu on groupMenu.menu_id equals menu.id
                      where groupMenu.group_id == groupId
                            && groupMenu.isActive != false
                            && groupMenu.isDeleted != true
                            && menu.menu_url == screenId
                            && menu.isActive != false
                            && menu.isDeleted != true
                      select menu.id).AnyAsync();
    }

    public Task<DynamicLoadResultDto> LoadScreenAsync(string screenId, DynamicLoadRequestDto request)
        => LoadScreenInternalAsync(screenId, request, _currentUser.UserId);

    public Task<DynamicLoadResultDto> LoadScreenAsUserAsync(string screenId, DynamicLoadRequestDto request, string userId)
        => LoadScreenInternalAsync(screenId, request, userId);

    private async Task<DynamicLoadResultDto> LoadScreenInternalAsync(string screenId, DynamicLoadRequestDto request, string? userId)
    {
        if (!await CanAccessScreenAsync(screenId, userId))
        {
            throw new UnauthorizedAccessException("Screen access is denied.");
        }

        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId
                                      && e.isActive != false
                                      && e.isDeleted != true
                                      && e.statusApproval == Const.StatusApproval.APPROVED)
            ?? throw new InvalidOperationException("Screen menu not found.");

        if (string.IsNullOrWhiteSpace(screen.query_load))
        {
            throw new InvalidOperationException("Screen menu has no query_load.");
        }

        return await ExecuteAsync(
            screen.query_load,
            screen.query_load_param,
            FieldFilterParser.Parse(screen.field_filter),
            request);
    }

    public async Task<List<DynamicOptionDto>> GetOptionsAsync(string screenId, string fieldId)
    {
        var screen = await _context.tb_screen_menu
            .FirstOrDefaultAsync(e => e.id == screenId && e.isDeleted != true)
            ?? throw new InvalidOperationException("Screen menu not found.");

        var field = FieldFilterParser.Parse(screen.field_filter)
            .FirstOrDefault(e => e.Id.Equals(fieldId, StringComparison.OrdinalIgnoreCase));
        if (field == null || !field.Type.Equals("l", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(field.Extra))
        {
            return new List<DynamicOptionDto>();
        }

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();
        var command = new SqlCommand(field.Extra, connection) { CommandTimeout = 60 };
        var options = new List<DynamicOptionDto>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            options.Add(new DynamicOptionDto
            {
                Value = reader.IsDBNull(0) ? string.Empty : Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture) ?? string.Empty,
                Text = reader.FieldCount > 1 && !reader.IsDBNull(1)
                    ? Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? string.Empty
                    : string.Empty,
            });
        }

        return options;
    }

    public async Task<DynamicReportDto?> GetReportAsync(string reportId)
    {
        var report = await _context.tb_report_set
            .FirstOrDefaultAsync(e => e.id == reportId
                                      && e.isActive != false
                                      && e.isDeleted != true
                                      && e.statusApproval == Const.StatusApproval.APPROVED);
        if (report == null || !await HasReportAccessAsync(reportId, _currentUser.UserId))
        {
            return null;
        }

        return new DynamicReportDto
        {
            ReportId = report.id,
            Name = report.name,
            Fields = FieldFilterParser.Parse(report.filter_param),
        };
    }

    public Task<DynamicLoadResultDto> LoadReportAsync(string reportId, DynamicLoadRequestDto request)
        => LoadReportInternalAsync(reportId, request, _currentUser.UserId);

    public Task<DynamicLoadResultDto> LoadReportAsUserAsync(string reportId, DynamicLoadRequestDto request, string userId)
        => LoadReportInternalAsync(reportId, request, userId);

    private async Task<DynamicLoadResultDto> LoadReportInternalAsync(string reportId, DynamicLoadRequestDto request, string? userId)
    {
        var report = await _context.tb_report_set
            .FirstOrDefaultAsync(e => e.id == reportId
                                      && e.isActive != false
                                      && e.isDeleted != true
                                      && e.statusApproval == Const.StatusApproval.APPROVED)
            ?? throw new InvalidOperationException("Report not found.");

        if (!await HasReportAccessAsync(reportId, userId))
        {
            throw new UnauthorizedAccessException("You do not have access to this report.");
        }

        if (string.IsNullOrWhiteSpace(report.query_list))
        {
            throw new InvalidOperationException("Report has no query_list.");
        }

        return await ExecuteAsync(
            report.query_list,
            report.query_list_param,
            FieldFilterParser.Parse(report.filter_param),
            request);
    }

    private async Task<bool> HasReportAccessAsync(string reportId, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var group = await _context.um_user_group
            .Where(e => e.user_id == userId && e.isDeleted != true)
            .Select(e => e.group_id)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(group))
        {
            return false;
        }

        return await _context.rf_group_access_report_set.AnyAsync(e =>
            e.group_id == group
            && e.report_set_id == reportId
            && e.isActive != false
            && e.isDeleted != true
            && e.statusApproval == Const.StatusApproval.APPROVED);
    }

    private async Task<DynamicLoadResultDto> ExecuteAsync(
        string query,
        string? queryParam,
        List<DynamicFieldDto> fields,
        DynamicLoadRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);
        var tokens = FieldFilterParser.ParseParams(queryParam);
        var hasPlaceholders = Regex.IsMatch(query, @"\{(\d+)\}");
        var isExec = Regex.IsMatch(query, @"^\s*(EXEC|EXECUTE)\b", RegexOptions.IgnoreCase);

        if (tokens.Any(e => e.IsNamed) && tokens.Any(e => !e.IsNamed))
        {
            throw new InvalidOperationException("Parameter mapping must be consistently positional or named.");
        }

        var values = new Dictionary<int, string?>();
        for (var index = 0; index < tokens.Count; index++)
        {
            values[index] = ResolveToken(tokens[index].Tag, fields, request);        }

        var likeModes = new Dictionary<int, string>();
        var preparedQuery = PreparePlaceholders(query, tokens, values, likeModes);
        if (hasPlaceholders && tokens.Count == 0)
        {
            throw new InvalidOperationException("query_load_param is required when the query contains placeholders.");
        }

        if (isExec)
        {
            return await ExecuteExecAsync(preparedQuery, tokens, values, likeModes, fields, page, pageSize, request, hasPlaceholders);
        }

        if (Regex.IsMatch(preparedQuery, @"\bORDER\s+BY\b", RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException("SELECT query must not contain ORDER BY; the engine applies paging and sorting.");
        }

        return await ExecuteSelectAsync(preparedQuery, tokens, values, likeModes, fields, page, pageSize, request);
    }

    private async Task<DynamicLoadResultDto> ExecuteSelectAsync(
        string query,
        List<DynamicParamToken> tokens,
        Dictionary<int, string?> values,
        Dictionary<int, string> likeModes,
        List<DynamicFieldDto> fields,
        int page,
        int pageSize,
        DynamicLoadRequestDto request)
    {
        var sortColumn = fields.FirstOrDefault(e => e.Id.Equals(request.SortColumn, StringComparison.OrdinalIgnoreCase))?.Id
                         ?? fields.FirstOrDefault()?.Id
                         ?? "1";
        if (!Regex.IsMatch(sortColumn, @"^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            sortColumn = "1";
        }

        var direction = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var offset = (page - 1) * pageSize;
        var countQuery = $"SELECT COUNT(*) FROM ({query}) q";

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        var schemaCommand = new SqlCommand($"SELECT TOP 0 * FROM ({query}) q", connection) { CommandTimeout = 120 };
        AddParameters(schemaCommand, tokens, values, likeModes, skipTotal: true);
        var resultColumns = new List<string>();
        await using (var schemaReader = await schemaCommand.ExecuteReaderAsync())
        {
            for (var index = 0; index < schemaReader.FieldCount; index++)
            {
                resultColumns.Add(schemaReader.GetName(index));
            }
        }

        var resolvedSort = resultColumns.FirstOrDefault(e => e.Equals(request.SortColumn, StringComparison.OrdinalIgnoreCase))
                           ?? resultColumns.FirstOrDefault()
                           ?? sortColumn;
        var orderBy = resolvedSort == "1" ? "(SELECT NULL)" : $"[{resolvedSort}]";
        var dataQuery = $"SELECT * FROM ({query}) q ORDER BY {orderBy} {direction} OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        var countCommand = new SqlCommand(countQuery, connection) { CommandTimeout = 120 };
        AddParameters(countCommand, tokens, values, likeModes, skipTotal: true);
        var total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(), CultureInfo.InvariantCulture);

        var dataCommand = new SqlCommand(dataQuery, connection) { CommandTimeout = 120 };
        dataCommand.Parameters.AddWithValue("@offset", offset);
        dataCommand.Parameters.AddWithValue("@pageSize", pageSize);
        AddParameters(dataCommand, tokens, values, likeModes, skipTotal: true);

        var (columns, rows) = await ReadRowsAsync(dataCommand);
        return new DynamicLoadResultDto
        {
            Fields = fields,
            Columns = columns,
            Rows = rows,
            TotalCount = total,
        };
    }

    private async Task<DynamicLoadResultDto> ExecuteExecAsync(
        string query,
        List<DynamicParamToken> tokens,
        Dictionary<int, string?> values,
        Dictionary<int, string> likeModes,
        List<DynamicFieldDto> fields,
        int page,
        int pageSize,
        DynamicLoadRequestDto request,
        bool hasPlaceholders)
    {
        var commandText = query.Trim().TrimEnd(';');
        var totalParameterName = "@totalOutput";
        var outputIndex = tokens.FindIndex(e => e.Tag.Equals("totalCount", StringComparison.OrdinalIgnoreCase));
        var named = tokens.Any(e => e.IsNamed);

        if (!hasPlaceholders && tokens.Count > 0)
        {
            var arguments = new List<string>();
            for (var index = 0; index < tokens.Count; index++)
            {
                if (index == outputIndex)
                {
                    continue;
                }

                if (named)
                {
                    if (string.IsNullOrWhiteSpace(tokens[index].Name) || !Regex.IsMatch(tokens[index].Name!, @"^@[A-Za-z_][A-Za-z0-9_]*$"))
                    {
                        throw new InvalidOperationException("Invalid named parameter.");
                    }

                    arguments.Add($"{tokens[index].Name} = @p{index}");
                }
                else
                {
                    arguments.Add($"@p{index}");
                }
            }

            if (outputIndex >= 0)
            {
                if (named)
                {
                    if (string.IsNullOrWhiteSpace(tokens[outputIndex].Name) || !Regex.IsMatch(tokens[outputIndex].Name!, @"^@[A-Za-z_][A-Za-z0-9_]*$"))
                    {
                        throw new InvalidOperationException("Invalid totalCount parameter name.");
                    }

                    arguments.Add($"{tokens[outputIndex].Name} = {totalParameterName} OUTPUT");
                }
                else
                {
                    arguments.Add($"{totalParameterName} OUTPUT");
                }
            }

            if (arguments.Count > 0)
            {
                commandText += " " + string.Join(", ", arguments);
            }
        }
        else if (outputIndex >= 0)
        {
            commandText += $" {totalParameterName} OUTPUT";
        }

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();
        var command = new SqlCommand(commandText, connection) { CommandTimeout = 120 };
        command.Parameters.AddWithValue("@page", page);
        command.Parameters.AddWithValue("@pageSize", pageSize);
        AddParameters(command, tokens, values, likeModes, skipTotal: true);
        var total = command.Parameters.Add(totalParameterName, SqlDbType.Int);
        total.Direction = ParameterDirection.Output;

        var (columns, rows) = await ReadRowsAsync(command);
        var totalCount = total.Value == DBNull.Value ? 0 : Convert.ToInt32(total.Value, CultureInfo.InvariantCulture);
        return new DynamicLoadResultDto
        {
            Fields = fields,
            Columns = columns,
            Rows = rows,
            TotalCount = totalCount,
        };
    }

    private static async Task<(List<string> Columns, List<Dictionary<string, object>> Rows)> ReadRowsAsync(SqlCommand command)
    {
        var columns = new List<string>();
        var rows = new List<Dictionary<string, object>>();
        await using var reader = await command.ExecuteReaderAsync();
        for (var index = 0; index < reader.FieldCount; index++)
        {
            columns.Add(reader.GetName(index));
        }

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < reader.FieldCount; index++)
            {
                row[reader.GetName(index)] = reader.IsDBNull(index)
                    ? string.Empty
                    : NormalizeValue(reader.GetValue(index)) ?? string.Empty;
            }

            rows.Add(row);
        }

        return (columns, rows);
    }

    private static object? NormalizeValue(object value) => value switch
    {
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        bool boolean => boolean,
        byte[] bytes => Convert.ToBase64String(bytes),
        _ => value,
    };

    private string? ResolveToken(string tag, List<DynamicFieldDto> fields, DynamicLoadRequestDto request)
    {
        return tag.ToLowerInvariant() switch
        {
            "page" => request.PageNumber.ToString(CultureInfo.InvariantCulture),
            "pagesize" => request.PageSize.ToString(CultureInfo.InvariantCulture),
            "searchterm" => request.SearchTerm,
            "sortcolumn" => request.SortColumn,
            "sortdirection" => string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC",
            "sessionuserid" => _currentUser.UserId,
            "entity" => null,
            "totalcount" => null,
            _ => ResolveFieldValue(tag, fields, request),
        };
    }

    private string? ResolveFieldValue(string tag, List<DynamicFieldDto> fields, DynamicLoadRequestDto request)
    {
        var field = fields.FirstOrDefault(e => e.Id.Equals(tag, StringComparison.OrdinalIgnoreCase));
        if (field != null && field.Type.Equals("h", StringComparison.OrdinalIgnoreCase)
            && field.Extra?.Equals("sessionuserid", StringComparison.OrdinalIgnoreCase) == true)
        {
            return _currentUser.UserId;
        }

        if (field != null && field.Type.Equals("h", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(field.Extra))
        {
            return field.Extra;
        }

        return request.Filters.GetValueOrDefault(tag);
    }

    private static string PreparePlaceholders(
        string query,
        List<DynamicParamToken> tokens,
        Dictionary<int, string?> values,
        Dictionary<int, string> likeModes)
    {
        var result = query;
        result = Regex.Replace(result, @"'%\s*\{(\d+)\}\s*%'", match =>
        {
            likeModes[int.Parse(match.Groups[1].Value)] = "both";
            return "@p" + match.Groups[1].Value;
        });
        result = Regex.Replace(result, @"'\{(\d+)\}\s*%'", match =>
        {
            likeModes[int.Parse(match.Groups[1].Value)] = "end";
            return "@p" + match.Groups[1].Value;
        });
        result = Regex.Replace(result, @"'%\s*\{(\d+)\}'", match =>
        {
            likeModes[int.Parse(match.Groups[1].Value)] = "start";
            return "@p" + match.Groups[1].Value;
        });
        result = Regex.Replace(result, @"'\{(\d+)\}'", match => "@p" + match.Groups[1].Value);
        result = Regex.Replace(result, @"\{(\d+)\}", match => "@p" + match.Groups[1].Value);
        return result;
    }

    private async Task<BaseGeneralResponse> ExecuteMutationAsync(
        string query,
        string? queryParam,
        List<DynamicFieldDto> fields,
        Dictionary<string, string?> values)
    {
        try
        {
            var tokens = FieldFilterParser.ParseParams(queryParam);
            var indexed = new Dictionary<int, string?>();
            for (var index = 0; index < tokens.Count; index++)
            {
                indexed[index] = ResolveMutationValue(tokens[index].Tag, fields, values);
            }

            var likeModes = new Dictionary<int, string>();
            var hasPlaceholders = Regex.IsMatch(query, @"\{(\d+)\}");
            var commandText = PreparePlaceholders(query.Trim(), tokens, indexed, likeModes);
            if (!hasPlaceholders && Regex.IsMatch(commandText, @"^\s*(EXEC|EXECUTE)\b", RegexOptions.IgnoreCase) && tokens.Count > 0)
            {
                commandText += " " + BuildExecArguments(tokens);
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            var command = new SqlCommand(commandText, connection) { CommandTimeout = 120 };
            AddParameters(command, tokens, indexed, likeModes, skipTotal: true);
            await command.ExecuteNonQueryAsync();
            return Success("Action completed.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteRowsAsync(
        string query,
        string? queryParam,
        List<DynamicFieldDto> fields,
        Dictionary<string, string?> values)
    {
        var tokens = FieldFilterParser.ParseParams(queryParam);
        var indexed = new Dictionary<int, string?>();
        for (var index = 0; index < tokens.Count; index++)
        {
            indexed[index] = ResolveMutationValue(tokens[index].Tag, fields, values);
        }

        var likeModes = new Dictionary<int, string>();
        var hasPlaceholders = Regex.IsMatch(query, @"\{(\d+)\}");
        var commandText = PreparePlaceholders(query.Trim().TrimEnd(';'), tokens, indexed, likeModes);
        if (!hasPlaceholders && Regex.IsMatch(commandText, @"^\s*(EXEC|EXECUTE)\b", RegexOptions.IgnoreCase) && tokens.Count > 0)
        {
            commandText += " " + BuildExecArguments(tokens);
        }

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();
        var command = new SqlCommand(commandText, connection) { CommandTimeout = 120 };
        AddParameters(command, tokens, indexed, likeModes, skipTotal: true);

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < reader.FieldCount; index++)
            {
                row[reader.GetName(index)] = reader.IsDBNull(index) ? null : NormalizeValue(reader.GetValue(index));
            }

            rows.Add(row);
        }

        return rows;
    }

    private string? ResolveMutationValue(string tag, List<DynamicFieldDto> fields, Dictionary<string, string?> values)
    {
        if (tag.Equals("sessionuserid", StringComparison.OrdinalIgnoreCase))
        {
            return _currentUser.UserId;
        }

        var field = fields.FirstOrDefault(e =>
            e.Id.Equals(tag, StringComparison.OrdinalIgnoreCase)
            || NormalizeKey(e.Id) == NormalizeKey(tag));
        var value = values.GetValueOrDefault(tag)
                    ?? (field != null ? values.GetValueOrDefault(field.Id) : null)
                    ?? values.FirstOrDefault(e => NormalizeKey(e.Key) == NormalizeKey(tag)).Value;

        if (field != null && (field.Type == "cb" || field.Type == "bit"))
        {
            return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ? "1" : "0";
        }

        return value;
    }

    private static string BuildExecArguments(List<DynamicParamToken> tokens)
    {
        var named = tokens.Any(e => e.IsNamed);
        var arguments = new List<string>();
        for (var index = 0; index < tokens.Count; index++)
        {
            if (tokens[index].Tag.Equals("totalCount", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (named)
            {
                if (string.IsNullOrWhiteSpace(tokens[index].Name)
                    || !Regex.IsMatch(tokens[index].Name!, @"^@[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    throw new InvalidOperationException("Invalid named parameter.");
                }

                arguments.Add($"{tokens[index].Name} = @p{index}");
            }
            else
            {
                arguments.Add($"@p{index}");
            }
        }

        return string.Join(", ", arguments);
    }

    private static string NormalizeKey(string key) => key.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

    private static void AddParameters(
        SqlCommand command,
        List<DynamicParamToken> tokens,
        Dictionary<int, string?> values,
        Dictionary<int, string> likeModes,
        bool skipTotal)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            if (skipTotal && tokens[index].Tag.Equals("totalCount", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = values.GetValueOrDefault(index);
            if (likeModes.TryGetValue(index, out var mode) && value != null)
            {
                value = mode switch
                {
                    "both" => $"%{value}%",
                    "start" => $"%{value}",
                    "end" => $"{value}%",
                    _ => value,
                };
            }

            command.Parameters.AddWithValue($"@p{index}", string.IsNullOrEmpty(value) ? DBNull.Value : value);
        }
    }

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = Const.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };
}
