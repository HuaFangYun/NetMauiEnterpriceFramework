using ClosedXML.Excel;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Group;
using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.DTOs.Parameter;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Services;

public class ManagementExportJob
{
    private readonly IUserManagementService _userService;
    private readonly IGroupManagementService _groupService;
    private readonly IRoleManagementService _roleService;
    private readonly IMenuManagementService _menuService;
    private readonly IParameterEngineService _parameterService;
    private readonly INotificationService _notificationService;
    private readonly IErrorLogService _errorLog;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ManagementExportJob> _logger;

    public ManagementExportJob(
        IUserManagementService userService,
        IGroupManagementService groupService,
        IRoleManagementService roleService,
        IMenuManagementService menuService,
        IParameterEngineService parameterService,
        INotificationService notificationService,
        IErrorLogService errorLog,
        IWebHostEnvironment environment,
        ILogger<ManagementExportJob> logger)
    {
        _userService = userService;
        _groupService = groupService;
        _roleService = roleService;
        _menuService = menuService;
        _parameterService = parameterService;
        _notificationService = notificationService;
        _errorLog = errorLog;
        _environment = environment;
        _logger = logger;
    }

    public async Task ExportUsersAsync(string? search, string userId, string jobId, string fileName)
    {
        try
        {
            var users = await LoadAllAsync(
                request => _userService.GetListAsync(request),
                search,
                "UserId");

            var headers = new[] { "User Id", "Full Name", "Gender", "Email", "Phone Number", "Role", "Status" };
            var rows = users.Select(user => new object?[]
            {
                user.UserId,
                user.FullName,
                user.GenderName,
                user.Email,
                user.PhoneNumber,
                user.GroupName,
                user.IsLocked ? "Locked" : user.IsActive ? "Active" : "Inactive",
            });

            WriteWorkbook(fileName, "Users", headers, rows);
            await NotifyReadyAsync(userId, "User export is ready to download.", jobId);
        }
        catch (Exception ex)
        {
            await LogFailureAsync("ExportUsers", userId, ex);
        }
    }

    public async Task ExportGroupsAsync(string? search, string userId, string jobId, string fileName)
    {
        try
        {
            var groups = await LoadAllAsync(
                request => _groupService.GetListAsync(request),
                search,
                "GroupId");

            var headers = new[] { "Group ID", "Group Name", "Users", "Status" };
            var rows = groups.Select(group => new object?[]
            {
                group.GroupId,
                group.GroupName,
                group.UserCount,
                group.IsActive ? "Active" : "Inactive",
            });

            WriteWorkbook(fileName, "Groups", headers, rows);
            await NotifyReadyAsync(userId, "Group export is ready to download.", jobId);
        }
        catch (Exception ex)
        {
            await LogFailureAsync("ExportGroups", userId, ex);
        }
    }

    public async Task ExportRolesAsync(string? search, string userId, string jobId, string fileName)
    {
        try
        {
            var roles = await LoadAllAsync(
                request => _roleService.GetListAsync(request),
                search,
                "UserId");

            var headers = new[] { "User ID", "Full Name", "Group", "Add/Edit", "Delete", "Read Only", "Status" };
            var rows = roles.Select(role => new object?[]
            {
                role.UserId,
                role.FullName,
                role.GroupName ?? role.GroupId,
                role.CanAddEdit ? "Yes" : "No",
                role.CanDelete ? "Yes" : "No",
                role.CanReadOnly ? "Yes" : "No",
                role.IsActive ? "Active" : "Inactive",
            });

            WriteWorkbook(fileName, "Roles", headers, rows);
            await NotifyReadyAsync(userId, "Role export is ready to download.", jobId);
        }
        catch (Exception ex)
        {
            await LogFailureAsync("ExportRoles", userId, ex);
        }
    }

    public async Task ExportMenusAsync(string? search, string userId, string jobId, string fileName)
    {
        try
        {
            var menus = await LoadAllAsync(
                request => _menuService.GetListAsync(request),
                search,
                "MenuUnique");

            var headers = new[] { "ID", "Unique", "Name", "Level", "Parent", "Order", "Status" };
            var rows = menus.Select(menu => new object?[]
            {
                menu.Id,
                menu.MenuUnique,
                menu.MenuName,
                menu.ParentLevelId,
                menu.ParentName ?? menu.MenuUniqueParent,
                menu.OrderSeq,
                menu.IsActive ? "Active" : "Inactive",
            });

            WriteWorkbook(fileName, "Menus", headers, rows);
            await NotifyReadyAsync(userId, "Menu export is ready to download.", jobId);
        }
        catch (Exception ex)
        {
            await LogFailureAsync("ExportMenus", userId, ex);
        }
    }

    public async Task ExportParametersAsync(string tableName, string? search, string userId, string jobId, string fileName)
    {
        try
        {
            var fields = await _parameterService.GetFieldsAsync(tableName);
            var rows = new List<Dictionary<string, string>>();
            var page = 1;

            while (true)
            {
                var result = await _parameterService.GetListAsync(tableName, new DataTableRequestDto
                {
                    PageNumber = page,
                    PageSize = 200,
                    SearchTerm = search,
                    SortDirection = "asc",
                });

                rows.AddRange(result.Rows);
                if (rows.Count >= result.TotalCount || result.Rows.Count == 0)
                {
                    break;
                }

                page++;
            }

            var headers = fields
                .Select(field => field.FieldDesc ?? field.FieldName)
                .ToArray();
            var dataRows = rows.Select(row => fields
                .Select(field => (object?)row.GetValueOrDefault(field.FieldName))
                .ToArray());

            WriteWorkbook(fileName, "Data", headers, dataRows);
            await NotifyReadyAsync(userId, "Parameter export is ready to download.", jobId);
        }
        catch (Exception ex)
        {
            await LogFailureAsync("ExportParameters", userId, ex);
        }
    }

    private static async Task<List<T>> LoadAllAsync<T>(
        Func<DataTableRequestDto, Task<GeneralListResponseDto<T>>> load,
        string? search,
        string sortColumn)
    {
        var items = new List<T>();
        var page = 1;

        while (true)
        {
            var result = await load(new DataTableRequestDto
            {
                PageNumber = page,
                PageSize = 200,
                SearchTerm = search,
                SortColumn = sortColumn,
                SortDirection = "asc",
            });

            items.AddRange(result.Data);
            if (items.Count >= result.TotalCount || result.Data.Count == 0)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private void WriteWorkbook(
        string fileName,
        string sheetName,
        IReadOnlyList<string> headers,
        IEnumerable<object?[]> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);

        for (var column = 0; column < headers.Count; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var column = 0; column < headers.Count; column++)
            {
                var value = column < row.Length ? row[column] : null;
                sheet.Cell(rowIndex, column + 1).Value = value switch
                {
                    null => string.Empty,
                    bool boolean => boolean ? "Yes" : "No",
                    IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
                    _ => value.ToString() ?? string.Empty,
                };
            }

            rowIndex++;
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();

        var exportsPath = Path.Combine(_environment.ContentRootPath, "exports");
        Directory.CreateDirectory(exportsPath);
        var finalPath = Path.Combine(exportsPath, fileName);
        var temporaryPath = finalPath + ".tmp.xlsx";
        workbook.SaveAs(temporaryPath);
        System.IO.File.Move(temporaryPath, finalPath, overwrite: true);
    }

    private Task NotifyReadyAsync(string userId, string message, string jobId)
        => _notificationService.CreateAsync(
            userId,
            "Export ready",
            message,
            $"/api/export/download-export/{jobId}",
            NotificationTypeIds.Export);

    private async Task LogFailureAsync(string entity, string userId, Exception exception)
    {
        _logger.LogError(exception, "{Entity} export failed for {UserId}", entity, userId);
        await _errorLog.LogAsync(entity, "ERROR", exception, userId);
    }
}
