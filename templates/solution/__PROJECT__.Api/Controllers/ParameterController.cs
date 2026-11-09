using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Parameter;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "00")]
public class ParameterController : ControllerBase
{
    private readonly IParameterEngineService _parameterService;

    public ParameterController(IParameterEngineService parameterService) => _parameterService = parameterService;

    [HttpGet("cards")]
    public async Task<IActionResult> GetCards() => Ok(await _parameterService.GetCardsAsync());

    [HttpGet("{tableName}/fields")]
    public async Task<IActionResult> GetFields(string tableName)
    {
        try
        {
            return Ok(await _parameterService.GetFieldsAsync(tableName));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{tableName}/options")]
    public async Task<IActionResult> GetOptions(string tableName)
    {
        try
        {
            return Ok(await _parameterService.GetOptionsAsync(tableName));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{tableName}/export")]
    public async Task<IActionResult> Export(string tableName, [FromQuery] string? search)
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

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Data");
            for (var column = 0; column < fields.Count; column++)
            {
                var cell = sheet.Cell(1, column + 1);
                cell.Value = fields[column].FieldDesc ?? fields[column].FieldName;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
            }

            for (var row = 0; row < rows.Count; row++)
            {
                for (var column = 0; column < fields.Count; column++)
                {
                    sheet.Cell(row + 2, column + 1).Value = rows[row].GetValueOrDefault(fields[column].FieldName) ?? string.Empty;
                }
            }

            sheet.SheetView.FreezeRows(1);
            sheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{tableName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{tableName}/list")]
    public async Task<IActionResult> GetList(string tableName, [FromBody] DataTableRequestDto request)
    {
        try
        {
            return Ok(await _parameterService.GetListAsync(tableName, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{tableName}/detail")]
    public async Task<IActionResult> GetDetail(string tableName, [FromQuery] string? key)
    {
        try
        {
            var row = await _parameterService.GetAsync(tableName, key);
            return row == null ? NotFound() : Ok(row);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] ParameterSaveRequestDto request)
    {
        var result = await _parameterService.SaveAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{tableName}")]
    public async Task<IActionResult> Delete(string tableName, [FromQuery] string? key)
    {
        var result = await _parameterService.DeleteAsync(tableName, key);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("pending/list")]
    public async Task<IActionResult> GetPending([FromBody] ParameterPendingRequestDto request)
        => Ok(await _parameterService.GetPendingAsync(request));

    [HttpPost("pending/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _parameterService.ApproveAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("pending/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _parameterService.RejectAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
