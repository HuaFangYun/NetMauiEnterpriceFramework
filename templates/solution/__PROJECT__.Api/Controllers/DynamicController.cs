using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.Dynamic;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DynamicController : ControllerBase
{
    private readonly IDynamicEngineService _dynamicService;

    public DynamicController(IDynamicEngineService dynamicService) => _dynamicService = dynamicService;

    [HttpGet("screen/{screenId}")]
    public async Task<IActionResult> GetScreen(string screenId)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        var screen = await _dynamicService.GetScreenAsync(screenId);
        return screen == null ? NotFound() : Ok(screen);
    }

    [HttpPost("screen/{screenId}/load")]
    public async Task<IActionResult> LoadScreen(string screenId, [FromBody] DynamicLoadRequestDto request)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        try
        {
            return Ok(await _dynamicService.LoadScreenAsync(screenId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("screen/{screenId}/options/{fieldId}")]
    public async Task<IActionResult> GetOptions(string screenId, string fieldId)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        try
        {
            return Ok(await _dynamicService.GetOptionsAsync(screenId, fieldId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("screen/{screenId}/child/detail")]
    public async Task<IActionResult> GetChildDetail(string screenId, [FromQuery] string key)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        try
        {
            var row = await _dynamicService.GetChildDetailAsync(screenId, key);
            return row == null ? NotFound() : Ok(row);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("screen/{screenId}/child/save")]
    public async Task<IActionResult> SaveChild(string screenId, [FromBody] DynamicMutationRequestDto request)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        var result = await _dynamicService.SaveChildAsync(screenId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("screen/{screenId}/process")]
    public async Task<IActionResult> Process(string screenId, [FromBody] DynamicMutationRequestDto request)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        var result = await _dynamicService.ProcessAsync(screenId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("screen/{screenId}/import-excel")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ImportExcel(string screenId, IFormFile file)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new BaseGeneralResponse { Success = false, Message = "Excel file is required." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _dynamicService.ImportExcelAsync(screenId, stream, file.FileName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("screen/{screenId}/delete")]
    public async Task<IActionResult> Delete(string screenId, [FromBody] DynamicMutationRequestDto request)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        var result = await _dynamicService.DeleteAsync(screenId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("screen/{screenId}/export-txt")]
    public async Task<IActionResult> ExportTxt(string screenId, [FromQuery] string? searchTerm, [FromQuery] string? sortColumn, [FromQuery] string? sortDirection)
    {
        if (!await _dynamicService.CanAccessScreenAsync(screenId))
        {
            return Forbid();
        }

        try
        {
            var result = await _dynamicService.ExportTxtAsync(screenId, new DynamicLoadRequestDto
            {
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
            });
            return File(Encoding.UTF8.GetBytes(result.Content), "text/plain; charset=utf-8", result.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("report/{reportId}")]
    public async Task<IActionResult> GetReport(string reportId)
    {
        var report = await _dynamicService.GetReportAsync(reportId);
        return report == null ? NotFound() : Ok(report);
    }

    [HttpPost("report/{reportId}/load")]
    public async Task<IActionResult> LoadReport(string reportId, [FromBody] DynamicLoadRequestDto request)
    {
        try
        {
            return Ok(await _dynamicService.LoadReportAsync(reportId, request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
