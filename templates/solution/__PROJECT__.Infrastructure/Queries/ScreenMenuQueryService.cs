using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Screen;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Queries;

public class ScreenMenuQueryService : IScreenMenuQueryService
{
    private readonly __PROJECT__DbContext _context;

    public ScreenMenuQueryService(__PROJECT__DbContext context) => _context = context;

    public async Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
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
            "id" => desc ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
            "description" => desc ? query.OrderByDescending(e => e.Description) : query.OrderBy(e => e.Description),
            "screenmenuchildid" => desc ? query.OrderByDescending(e => e.ScreenMenuChildId) : query.OrderBy(e => e.ScreenMenuChildId),
            "screenmenuimportexcelprocessid" => desc ? query.OrderByDescending(e => e.ScreenMenuImportExcelProcessId) : query.OrderBy(e => e.ScreenMenuImportExcelProcessId),
            "modifieddate" => desc ? query.OrderByDescending(e => e.ModifiedDate) : query.OrderBy(e => e.ModifiedDate),
            _ => desc ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
        };

        return new GeneralListResponseDto<ScreenMenuListItemDto>
        {
            Data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken),
            TotalCount = await query.CountAsync(cancellationToken),
        };
    }

    public async Task<ScreenMenuEditDto?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var screen = await _context.tb_screen_menu.FirstOrDefaultAsync(
            e => e.id == id && e.isDeleted != true,
            cancellationToken);
        if (screen == null)
        {
            return null;
        }

        var child = await _context.tb_screen_menu_child.FirstOrDefaultAsync(
            e => e.screen_menu_child_id == screen.screen_menu_child_id && e.isDeleted != true,
            cancellationToken);
        var import = await _context.tb_screen_menu_import_excel_process.FirstOrDefaultAsync(
            e => e.screen_menu_import_excel_process_id == screen.screen_menu_import_excel_process_id && e.isDeleted != true,
            cancellationToken);

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
}
