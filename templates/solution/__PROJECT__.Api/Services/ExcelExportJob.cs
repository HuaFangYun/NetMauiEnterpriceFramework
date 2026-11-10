using System.Globalization;

using ClosedXML.Excel;

using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Services;

public class ExcelExportJob
{
    private readonly IDynamicEngineService _dynamicEngine;
    private readonly INotificationService _notificationService;
    private readonly IErrorLogService _errorLog;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExcelExportJob> _logger;

    public ExcelExportJob(
        IDynamicEngineService dynamicEngine,
        INotificationService notificationService,
        IErrorLogService errorLog,
        IWebHostEnvironment environment,
        ILogger<ExcelExportJob> logger)
    {
        _dynamicEngine = dynamicEngine;
        _notificationService = notificationService;
        _errorLog = errorLog;
        _environment = environment;
        _logger = logger;
    }

    public async Task ExportReportAsync(string reportId, DynamicLoadRequestDto request, string userId, string fileName)
    {
        try
        {
            var (columns, rows) = await LoadReportRowsAsync(reportId, request, userId);
            WriteWorkbook(fileName, reportId, columns, rows);
            await _notificationService.CreateAsync(
                userId,
                "Export ready",
                $"Report {reportId} is ready to download.",
                $"/api/export/download/{fileName}",
                NotificationTypeIds.Export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report export failed for {ReportId}", reportId);
            await _errorLog.LogAsync("ExportReport", "ERROR", ex, userId);
        }
    }

    public async Task ExportScreenAsync(string screenId, DynamicLoadRequestDto request, string userId, string fileName)
    {
        try
        {
            var (columns, rows) = await LoadScreenRowsAsync(screenId, request, userId);
            WriteWorkbook(fileName, screenId, columns, rows);
            await _notificationService.CreateAsync(
                userId,
                "Export ready",
                $"Screen {screenId} is ready to download.",
                $"/api/export/download/{fileName}",
                NotificationTypeIds.Export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Screen export failed for {ScreenId}", screenId);
            await _errorLog.LogAsync("ExportScreen", "ERROR", ex, userId);
        }
    }

    private async Task<(List<string> Columns, List<Dictionary<string, object>> Rows)> LoadReportRowsAsync(
        string reportId,
        DynamicLoadRequestDto request,
        string userId)
    {
        var rows = new List<Dictionary<string, object>>();
        var columns = new List<string>();
        var page = 1;

        while (true)
        {
            request.PageNumber = page;
            request.PageSize = 200;
            var result = await _dynamicEngine.LoadReportAsUserAsync(reportId, request, userId);
            columns = result.Columns;
            rows.AddRange(result.Rows);

            if (rows.Count >= result.TotalCount || result.Rows.Count == 0)
            {
                break;
            }

            page++;
        }

        return (columns, rows);
    }

    private async Task<(List<string> Columns, List<Dictionary<string, object>> Rows)> LoadScreenRowsAsync(
        string screenId,
        DynamicLoadRequestDto request,
        string userId)
    {
        var rows = new List<Dictionary<string, object>>();
        var columns = new List<string>();
        var page = 1;

        while (true)
        {
            request.PageNumber = page;
            request.PageSize = 200;
            var result = await _dynamicEngine.LoadScreenAsUserAsync(screenId, request, userId);
            columns = result.Columns;
            rows.AddRange(result.Rows);

            if (rows.Count >= result.TotalCount || result.Rows.Count == 0)
            {
                break;
            }

            page++;
        }

        return (columns, rows);
    }

    private void WriteWorkbook(
        string fileName,
        string title,
        List<string> columns,
        List<Dictionary<string, object>> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Data");

        for (var column = 0; column < columns.Count; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = columns[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }

        for (var row = 0; row < rows.Count; row++)
        {
            for (var column = 0; column < columns.Count; column++)
            {
                var value = rows[row].GetValueOrDefault(columns[column]);
                sheet.Cell(row + 2, column + 1).Value = value switch
                {
                    null => string.Empty,
                    bool boolean => boolean ? "Yes" : "No",
                    IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                    _ => value.ToString() ?? string.Empty,
                };
            }
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
}
