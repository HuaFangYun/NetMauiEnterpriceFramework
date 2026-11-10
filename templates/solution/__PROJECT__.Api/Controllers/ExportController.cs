using System.Security.Claims;
using System.Text.RegularExpressions;

using Hangfire;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Api.Services;
using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.Export;
using __PROJECT__.Application.DTOs.General;

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

    [HttpPost("users")]
    [Authorize(Roles = "00")]
    public IActionResult QueueUsers([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var jobId = Guid.NewGuid().ToString("N");
        var fileName = $"users_{jobId}.xlsx";
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportUsersAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("groups")]
    [Authorize(Roles = "00")]
    public IActionResult QueueGroups([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var jobId = Guid.NewGuid().ToString("N");
        var fileName = $"groups_{jobId}.xlsx";
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportGroupsAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("roles")]
    [Authorize(Roles = "00")]
    public IActionResult QueueRoles([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var jobId = Guid.NewGuid().ToString("N");
        var fileName = $"roles_{jobId}.xlsx";
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportRolesAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("menus")]
    [Authorize(Roles = "00")]
    public IActionResult QueueMenus([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var jobId = Guid.NewGuid().ToString("N");
        var fileName = $"menus_{jobId}.xlsx";
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportMenusAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("parameters/{tableName}")]
    [Authorize(Roles = "00")]
    public IActionResult QueueParameters(string tableName, [FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var jobId = Guid.NewGuid().ToString("N");
        var fileName = $"parameters_{jobId}.xlsx";
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportParametersAsync(tableName, request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpGet("status/{jobId}")]
    public IActionResult GetExportStatus(string jobId)
    {
        var file = FindExportFile(jobId);
        return Ok(ApiResponseFactory.Success(new ExportStatusDto
        {
            Ready = file != null,
            FileName = file == null ? null : Path.GetFileName(file),
            Message = file == null ? "Export is still processing." : "Export is ready.",
        }, file == null ? "Export is still processing." : "Export is ready."));
    }

    [HttpGet("download-export/{jobId}")]
    public IActionResult DownloadExport(string jobId)
    {
        var file = FindExportFile(jobId);
        if (file == null)
        {
            return NotFound(ApiResponseFactory.NotFound<string>("Export file not found."));
        }

        var bytes = System.IO.File.ReadAllBytes(file);
        var fileName = Path.GetFileName(file);
        System.IO.File.Delete(file);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
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

    private string? CurrentUserId()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private IActionResult Queued(string jobId, string fileName)
        => Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));

    private string? FindExportFile(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId) || jobId.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return null;
        }

        var folder = Path.Combine(_environment.ContentRootPath, "exports");
        if (!Directory.Exists(folder))
        {
            return null;
        }

        return Directory.GetFiles(folder, $"*_{jobId}.xlsx").FirstOrDefault();
    }

    private static string Sanitize(string value)
    {
        var sanitized = new string(value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "export" : sanitized;
    }
}
