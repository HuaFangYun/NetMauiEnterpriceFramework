using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Group;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "00")]
public class GroupController : ControllerBase
{
    private readonly IGroupManagementService _groupService;

    public GroupController(IGroupManagementService groupService) => _groupService = groupService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _groupService.GetListAsync(request));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search)
    {
        var items = new List<GroupListItemDto>();
        var page = 1;
        while (true)
        {
            var result = await _groupService.GetListAsync(new DataTableRequestDto
            {
                PageNumber = page,
                PageSize = 200,
                SearchTerm = search,
                SortColumn = "GroupId",
                SortDirection = "asc",
            });
            items.AddRange(result.Data);
            if (items.Count >= result.TotalCount || result.Data.Count == 0)
            {
                break;
            }
            page++;
        }

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Groups");
        var headers = new[] { "Group ID", "Group Name", "Users", "Status" };
        WriteHeader(sheet, headers);

        for (var row = 0; row < items.Count; row++)
        {
            var item = items[row];
            sheet.Cell(row + 2, 1).Value = item.GroupId;
            sheet.Cell(row + 2, 2).Value = item.GroupName ?? string.Empty;
            sheet.Cell(row + 2, 3).Value = item.UserCount;
            sheet.Cell(row + 2, 4).Value = item.IsActive ? "Active" : "Inactive";
        }

        return WorkbookResult(workbook, "groups");
    }

    private static void WriteHeader(IXLWorksheet sheet, string[] headers)
    {
        for (var column = 0; column < headers.Length; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static FileContentResult WorkbookResult(XLWorkbook workbook, string prefix)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new FileContentResult(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
        };
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> Get(string groupId)
    {
        var group = await _groupService.GetByIdAsync(groupId);
        return group == null ? NotFound() : Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GroupEditDto dto)
    {
        var result = await _groupService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> Update(string groupId, [FromBody] GroupEditDto dto)
    {
        var result = await _groupService.UpdateAsync(groupId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> Delete(string groupId)
    {
        var result = await _groupService.DeleteAsync(groupId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
