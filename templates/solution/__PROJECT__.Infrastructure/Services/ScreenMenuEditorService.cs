using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Screen;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Infrastructure.Services;

public class ScreenMenuEditorService : IScreenMenuEditorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScreenMenuQueryService _queryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;

    public ScreenMenuEditorService(
        IUnitOfWork unitOfWork,
        IScreenMenuQueryService queryService,
        ICurrentUserService currentUser,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _currentUser = currentUser;
        _errorLog = errorLog;
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("ScreenMenuEditor", "ERROR", exception);

    public Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(DataTableRequestDto request)
        => _queryService.GetListAsync(request);

    public Task<ScreenMenuEditDto?> GetAsync(string id)
        => _queryService.GetAsync(id);

    public async Task<BaseGeneralResponse> StageAsync(ScreenMenuEditDto dto)
    {
        try
        {
            dto.Id = dto.Id.Trim();
            if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return Fail("Screen id and description are required.");
            }

            var exists = await _unitOfWork.ScreenMenus.ExistsAsync(dto.Id);
            var type = exists ? Const.ApprovalType.MODIFIED : Const.ApprovalType.ADD;
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
                var childExists = await _unitOfWork.ScreenMenus.ChildExistsAsync(dto.Child.ScreenMenuChildId);
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
                var importExists = await _unitOfWork.ScreenMenus.ImportExistsAsync(dto.Import.ScreenMenuImportExcelProcessId);
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
            var details = values.Select(pair => new tb_parameter_sytem_temporary_detail
            {
                tb_parameter_sytem_temporary_id = temporary.id,
                column_name = pair.Key,
                value = pair.Value,
            });

            await _unitOfWork.ParameterStaging.AddAsync(temporary, details);
            await _unitOfWork.SaveChangesAsync();
            return Success("Screen menu request sent for approval.");
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex);
            return Fail(ex.Message);
        }
    }

    private Task<bool> HasPendingAsync(string key)
        => _unitOfWork.ParameterStaging.HasPendingKeyAsync("tb_screen_menu", "__key", key);

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
