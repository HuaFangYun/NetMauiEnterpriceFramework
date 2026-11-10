using System.Security.Claims;
using System.Text.RegularExpressions;

using Hangfire;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Api.Services;
using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.Export;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private static readonly Regex SafeFileName = new(@"^[A-Za-z0-9_.-]+$", RegexOptions.Compiled);

    private readonly IBackgroundJobClient _jobs;
    private readonly IWebHostEnvironment _environment;

    public ExportController(IBackgroundJobClient jobs, IWebHostEnvironment environment)
    {
        _jobs = jobs;
        _environment = environment;
    }

    [HttpPost("report/{reportId}")]
    public IActionResult QueueReport(string reportId, [FromBody] DynamicLoadRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var fileName = $"report_{Sanitize(reportId)}_{Guid.NewGuid():N}.xlsx";
        var jobId = _jobs.Enqueue<ExcelExportJob>(job =>
            job.ExportReportAsync(reportId, request, userId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpPost("screen/{screenId}")]
    public IActionResult QueueScreen(string screenId, [FromBody] DynamicLoadRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var fileName = $"screen_{Sanitize(screenId)}_{Guid.NewGuid():N}.xlsx";
        var jobId = _jobs.Enqueue<ExcelExportJob>(job =>
            job.ExportScreenAsync(screenId, request, userId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpPost("pdf/report/{reportId}")]
    public IActionResult QueuePdfReport(string reportId, [FromBody] DynamicLoadRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var fileName = $"report_{Sanitize(reportId)}_{Guid.NewGuid():N}.pdf";
        var jobId = _jobs.Enqueue<PdfExportJob>(job =>
            job.ExportReportAsync(reportId, request, userId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpPost("pdf/screen/{screenId}")]
    public IActionResult QueuePdfScreen(string screenId, [FromBody] DynamicLoadRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var fileName = $"screen_{Sanitize(screenId)}_{Guid.NewGuid():N}.pdf";
        var jobId = _jobs.Enqueue<PdfExportJob>(job =>
            job.ExportScreenAsync(screenId, request, userId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpGet("download/{fileName}")]
    public IActionResult Download(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!SafeFileName.IsMatch(fileName) || extension is not (".xlsx" or ".pdf"))
        {
            return BadRequest();
        }

        var path = Path.Combine(_environment.ContentRootPath, "exports", fileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var contentType = extension == ".pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return PhysicalFile(path, contentType, fileName);
    }

    private static string Sanitize(string value)
    {
        var sanitized = new string(value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "export" : sanitized;
    }
}
