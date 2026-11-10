using System.Globalization;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Services;

public class PdfExportJob
{
    private readonly IDynamicEngineService _dynamicEngine;
    private readonly INotificationService _notificationService;
    private readonly IErrorLogService _errorLog;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PdfExportJob> _logger;

    public PdfExportJob(
        IDynamicEngineService dynamicEngine,
        INotificationService notificationService,
        IErrorLogService errorLog,
        IWebHostEnvironment environment,
        ILogger<PdfExportJob> logger)
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
            WritePdf(fileName, reportId, columns, rows);
            await _notificationService.CreateAsync(
                userId,
                "PDF export ready",
                $"Report {reportId} PDF is ready to download.",
                $"/api/export/download/{fileName}",
                NotificationTypeIds.Export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report PDF export failed for {ReportId}", reportId);
            await _errorLog.LogAsync("ExportPdfReport", "ERROR", ex, userId);
        }
    }

    public async Task ExportScreenAsync(string screenId, DynamicLoadRequestDto request, string userId, string fileName)
    {
        try
        {
            var (columns, rows) = await LoadScreenRowsAsync(screenId, request, userId);
            WritePdf(fileName, screenId, columns, rows);
            await _notificationService.CreateAsync(
                userId,
                "PDF export ready",
                $"Screen {screenId} PDF is ready to download.",
                $"/api/export/download/{fileName}",
                NotificationTypeIds.Export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Screen PDF export failed for {ScreenId}", screenId);
            await _errorLog.LogAsync("ExportPdfScreen", "ERROR", ex, userId);
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

    private void WritePdf(
        string fileName,
        string title,
        List<string> columns,
        List<Dictionary<string, object>> rows)
    {
        var exportsPath = Path.Combine(_environment.ContentRootPath, "exports");
        Directory.CreateDirectory(exportsPath);
        var finalPath = Path.Combine(exportsPath, fileName);
        var temporaryPath = finalPath + ".tmp.pdf";
        var pageSize = columns.Count > 18 ? PageSizes.A3.Landscape() : PageSizes.A4.Landscape();
        var fontSize = columns.Count switch
        {
            <= 6 => 8f,
            <= 10 => 7f,
            <= 15 => 6f,
            <= 20 => 5f,
            _ => 4.5f,
        };

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.Margin(20);
                page.DefaultTextStyle(style => style.FontSize(fontSize));

                page.Header().Column(header =>
                {
                    header.Item().Text(title).FontSize(16).SemiBold();
                    header.Item().Text($"{rows.Count} row(s) • generated {DateTime.Now:yyyy-MM-dd HH:mm}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(8).Table(table =>
                {
                    table.ColumnsDefinition(columnsDefinition =>
                    {
                        foreach (var _ in columns)
                        {
                            columnsDefinition.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var column in columns)
                        {
                            header.Cell()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(3)
                                .Text(column)
                                .SemiBold();
                        }
                    });

                    foreach (var row in rows.Take(5000))
                    {
                        foreach (var column in columns)
                        {
                            table.Cell()
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(3)
                                .Text(FormatValue(row.GetValueOrDefault(column)));
                        }
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(temporaryPath);

        System.IO.File.Move(temporaryPath, finalPath, overwrite: true);
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        bool boolean => boolean ? "Yes" : "No",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
        _ => value.ToString() ?? string.Empty,
    };
}
