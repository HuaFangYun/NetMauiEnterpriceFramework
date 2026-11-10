using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Parameter;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "00")]
public class ParameterController : ControllerBase
{
    private readonly IParameterEngineService _parameterService;

    public ParameterController(IParameterEngineService parameterService) => _parameterService = parameterService;

    [HttpGet("systems")]
    [HttpGet("cards")]
    public async Task<IActionResult> GetSystems()
    {
        var cards = await _parameterService.GetCardsAsync();
        return Ok(ApiResponseFactory.Success(cards, "Parameter systems retrieved successfully"));
    }

    [HttpGet("{tableName}/fields")]
    [HttpGet("fields/{tableName}")]
    public async Task<IActionResult> GetFields(string tableName)
    {
        try
        {
            var fields = await _parameterService.GetFieldsAsync(tableName);
            return Ok(ApiResponseFactory.Success(fields, "Parameter fields retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.Error<List<ParameterFieldDto>>(ex.Message));
        }
    }

    [HttpGet("{tableName}/options")]
    public async Task<IActionResult> GetOptions(string tableName)
    {
        try
        {
            var options = await _parameterService.GetOptionsAsync(tableName);
            return Ok(ApiResponseFactory.Success(options, "Parameter options retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.Error<Dictionary<string, List<DropdownItemDto>>>(ex.Message));
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
                $"parameter_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.Error<string>(ex.Message));
        }
    }

    [HttpPost("approval-list/{tableName}")]
    public async Task<IActionResult> GetApprovalRows(string tableName, [FromBody] DataTableRequestDto request)
    {
        try
        {
            var result = await _parameterService.GetApprovalRowsAsync(tableName, request);
            return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Parameter approvals retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.Error<PaginatedResponse<ParameterApprovalRowDto>>(ex.Message));
        }
    }

    [HttpPost("{tableName}/list")]
    public async Task<IActionResult> GetList(string tableName, [FromBody] DataTableRequestDto request)
    {
        try
        {
            var result = await _parameterService.GetListAsync(tableName, request);
            return Ok(new ParameterListResponseDto
            {
                Success = true,
                Message = "Parameters retrieved successfully",
                Code = Const.SUCCESS_CODE,
                Data = result.Rows,
                Fields = result.Fields,
                KeyField = result.KeyField,
                Pagination = new PaginationInfo
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = result.TotalCount,
                },
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.Error<ParameterListResponseDto>(ex.Message));
        }
    }

    [HttpGet("{tableName}/detail")]
    public async Task<IActionResult> GetDetail(string tableName, [FromQuery] string? key)
    {
        try
        {
            var row = await _parameterService.GetAsync(tableName, key);
            return row == null
                ? NotFound(ApiResponseFactory.NotFound<Dictionary<string, string>>("Parameter not found."))
                : Ok(ApiResponseFactory.Success(row, "Parameter retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.Error<Dictionary<string, string>>(ex.Message));
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] ParameterSaveRequestDto request)
        => ToActionResult(await _parameterService.SaveAsync(request));

    [HttpDelete("{tableName}")]
    public async Task<IActionResult> Delete(string tableName, [FromQuery] string? key)
        => ToActionResult(await _parameterService.DeleteAsync(tableName, key));

    [HttpPost("pending/list")]
    public async Task<IActionResult> GetPending([FromBody] ParameterPendingRequestDto request)
    {
        var result = await _parameterService.GetPendingAsync(request);
        var pageRequest = new DataTableRequestDto
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            SortColumn = request.SortColumn,
            SortDirection = request.SortDirection,
        };
        return Ok(ApiResponseFactory.Page(result.Data, pageRequest, result.TotalCount, "Pending parameters retrieved successfully"));
    }

    [HttpPost("pending/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
        => ToActionResult(await _parameterService.ApproveAsync(id));

    [HttpPost("pending/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
        => ToActionResult(await _parameterService.RejectAsync(id));

    private IActionResult ToActionResult(BaseGeneralResponse result)
    {
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
