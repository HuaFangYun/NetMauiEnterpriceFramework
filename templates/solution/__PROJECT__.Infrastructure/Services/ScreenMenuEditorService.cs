using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Screen;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class ScreenMenuEditorService : IScreenMenuEditorService
{
    private readonly __PROJECT__DbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;

    public ScreenMenuEditorService(
        __PROJECT__DbContext context,
        ICurrentUserService currentUser,
        IErrorLogService errorLog)
    {
        _context = context;
        _currentUser = currentUser;
        _errorLog = errorLog;
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("ScreenMenuEditor", "ERROR", exception);

    public async Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(DataTableRequestDto request)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = _context.tb_screen_menu
            .Where(e => e.isDeleted != true)
            .Select(e => new ScreenMenuListItemDto
            {
                Id = e.id,
                Description = e.description,
                IsActive = e.isActive == true,
                StatusApproval = e.statusApproval,
                ScreenMenuChildId = e.screen_menu_child_id,
                ScreenMenuImportExcelProcessId = e.screen_menu_import_excel_process_id,
                ModifiedDate = e.modifiedDate ?? e.createdDate,
            });

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(e => e.Id.Contains(search) || (e.Description != null && e.Description.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "description" => desc ? query.OrderByDescending(e => e.Description) : query.OrderBy(e => e.Description),
            "modifieddate" => desc ? query.OrderByDescending(e => e.ModifiedDate) : query.OrderBy(e => e.ModifiedDate),
            _ => desc ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
        };

        return new GeneralListResponseDto<ScreenMenuListItemDto>
        {
            Data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
            TotalCount = await query.CountAsync(),
        };
    }

    public async Task<ScreenMenuEditDto?> GetAsync(string id)
    {
        var screen = await _context.tb_screen_menu.FirstOrDefaultAsync(e => e.id == id && e.isDeleted != true);
        if (screen == null)
        {
            return null;
        }

        var child = await _context.tb_screen_menu_child.FirstOrDefaultAsync(e => e.screen_menu_child_id == screen.screen_menu_child_id && e.isDeleted != true);
        var import = await _context.tb_screen_menu_import_excel_process.FirstOrDefaultAsync(e => e.screen_menu_import_excel_process_id == screen.screen_menu_import_excel_process_id && e.isDeleted != true);

        return new ScreenMenuEditDto
        {
            Id = screen.id,
            Description = screen.description,
            HasButtonLoad = screen.has_button_load == true,
            HasButtonProcess = screen.has_button_process == true,
            HasButtonGenerateTxt = screen.has_button_generate_txt == true,
            HasButtonExportExcel = screen.has_button_export_excel == true,
            HasButtonAdd = screen.has_button_add == true,
            HasButtonDelete = screen.has_button_delete == true,
            HasActionRow = screen.has_action_row == true,
            HasButtonImportExcel = screen.has_button_import_excel == true,
            FieldFilter = screen.field_filter,
            QueryLoad = screen.query_load,
            QueryLoadParam = screen.query_load_param,
            QueryProcess = screen.query_process,
            QueryProcessParam = screen.query_process_param,
            QueryExportTxt = screen.query_export_txt,
            QueryExportTxtParam = screen.query_export_txt_param,
            QueryDelete = screen.query_delete,
            QueryDeleteParam = screen.query_delete_param,
            IsActive = screen.isActive == true,
            ScreenMenuChildId = screen.screen_menu_child_id,
            ScreenMenuImportExcelProcessId = screen.screen_menu_import_excel_process_id,
            Child = child == null ? null : new ScreenMenuChildEditDto
            {
                ScreenMenuChildId = child.screen_menu_child_id,
                Description = child.description,
                Type = child.type,
                UrlAction = child.url_action,
                UrlActionParam = child.url_action_param,
                ModalField = child.modal_field,
                ModalDetailFieldQuery = child.modal_detail_field_query,
                ModalDetailFieldQueryParam = child.modal_detail_field_query_param,
                ModalSaveProcessQuery = child.modal_save_process_query,
                ModalSaveProcessQueryParam = child.modal_save_process_query_param,
                IsActive = child.isActive == true,
            },
            Import = import == null ? null : new ScreenMenuImportEditDto
            {
                ScreenMenuImportExcelProcessId = import.screen_menu_import_excel_process_id,
                Description = import.description,
                TableName = import.table_name,
                FieldInput = import.field_input,
                WithApproval = import.with_approval == true,
                IsActive = import.isActive == true,
            },
        };
    }

    public async Task<BaseGeneralResponse> StageAsync(ScreenMenuEditDto dto)
    {
        try
        {
            dto.Id = dto.Id.Trim();
            if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return Fail("Screen id and description are required.");
            }

            var existing = await _context.tb_screen_menu.FirstOrDefaultAsync(e => e.id == dto.Id);
            var type = existing == null ? Const.ApprovalType.ADD : Const.ApprovalType.MODIFIED;
            if (await HasPendingAsync(dto.Id))
            {
                return Fail("Screen menu already has a pending request.");
            }

            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => values[$"screen.{key}"] = value;

            Add("id", dto.Id);
            Add("description", dto.Description);
            Add("has_button_load", Bool(dto.HasButtonLoad));
            Add("has_button_process", Bool(dto.HasButtonProcess));
            Add("has_button_generate_txt", Bool(dto.HasButtonGenerateTxt));
            Add("has_button_export_excel", Bool(dto.HasButtonExportExcel));
            Add("has_button_add", Bool(dto.HasButtonAdd));
            Add("has_button_delete", Bool(dto.HasButtonDelete));
            Add("has_action_row", Bool(dto.HasActionRow));
            Add("has_button_import_excel", Bool(dto.HasButtonImportExcel));
            Add("field_filter", dto.FieldFilter);
            Add("query_load", dto.QueryLoad);
            Add("query_load_param", dto.QueryLoadParam);
            Add("query_process", dto.QueryProcess);
            Add("query_process_param", dto.QueryProcessParam);
            Add("query_export_txt", dto.QueryExportTxt);
            Add("query_export_txt_param", dto.QueryExportTxtParam);
            Add("query_delete", dto.QueryDelete);
            Add("query_delete_param", dto.QueryDeleteParam);
            Add("is_active", Bool(dto.IsActive));
            Add("screen_menu_child_id", dto.ScreenMenuChildId);
            Add("screen_menu_import_excel_process_id", dto.ScreenMenuImportExcelProcessId);
            values["__key"] = dto.Id;

            if (dto.Child != null && !string.IsNullOrWhiteSpace(dto.Child.ScreenMenuChildId))
            {
                var childExists = await _context.tb_screen_menu_child.AnyAsync(e => e.screen_menu_child_id == dto.Child.ScreenMenuChildId);
                values["__child_type"] = childExists ? Const.ApprovalType.MODIFIED : Const.ApprovalType.ADD;
                void AddChild(string key, string? value) => values[$"child.{key}"] = value;
                AddChild("screen_menu_child_id", dto.Child.ScreenMenuChildId);
                AddChild("description", dto.Child.Description);
                AddChild("type", dto.Child.Type);
                AddChild("url_action", dto.Child.UrlAction);
                AddChild("url_action_param", dto.Child.UrlActionParam);
                AddChild("modal_field", dto.Child.ModalField);
                AddChild("modal_detail_field_query", dto.Child.ModalDetailFieldQuery);
                AddChild("modal_detail_field_query_param", dto.Child.ModalDetailFieldQueryParam);
                AddChild("modal_save_process_query", dto.Child.ModalSaveProcessQuery);
                AddChild("modal_save_process_query_param", dto.Child.ModalSaveProcessQueryParam);
                AddChild("is_active", Bool(dto.Child.IsActive));
            }

            if (dto.Import != null && !string.IsNullOrWhiteSpace(dto.Import.ScreenMenuImportExcelProcessId))
            {
                var importExists = await _context.tb_screen_menu_import_excel_process.AnyAsync(e => e.screen_menu_import_excel_process_id == dto.Import.ScreenMenuImportExcelProcessId);
                values["__import_type"] = importExists ? Const.ApprovalType.MODIFIED : Const.ApprovalType.ADD;
                void AddImport(string key, string? value) => values[$"import.{key}"] = value;
                AddImport("screen_menu_import_excel_process_id", dto.Import.ScreenMenuImportExcelProcessId);
                AddImport("description", dto.Import.Description);
                AddImport("table_name", dto.Import.TableName);
                AddImport("field_input", dto.Import.FieldInput);
                AddImport("with_approval", Bool(dto.Import.WithApproval));
                AddImport("is_active", Bool(dto.Import.IsActive));
            }

            var temporary = new tb_parameter_sytem_temporary
            {
                id = Guid.NewGuid(),
                table_name = "tb_screen_menu",
                type = type,
                created_by = _currentUser.UserId ?? "system",
                created_date = DateTime.Now,
            };
            await _context.tb_parameter_sytem_temporary.AddAsync(temporary);

            foreach (var pair in values)
            {
                await _context.tb_parameter_sytem_temporary_detail.AddAsync(new tb_parameter_sytem_temporary_detail
                {
                    tb_parameter_sytem_temporary_id = temporary.id,
                    column_name = pair.Key,
                    value = pair.Value,
                });
            }

            await _context.SaveChangesAsync();
            return Success("Screen menu request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    private async Task<bool> HasPendingAsync(string key)
        => await (from temporary in _context.tb_parameter_sytem_temporary
                  join detail in _context.tb_parameter_sytem_temporary_detail
                      on temporary.id equals detail.tb_parameter_sytem_temporary_id
                  where temporary.table_name == "tb_screen_menu"
                        && detail.column_name == "__key"
                        && detail.value == key
                  select temporary.id).AnyAsync();

    private static string Bool(bool value) => value ? "true" : "false";

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
