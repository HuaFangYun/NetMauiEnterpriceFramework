using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "00")]
public class UserGroupController : ControllerBase
{
    private readonly IRoleManagementService _roleService;

    public UserGroupController(IRoleManagementService roleService) => _roleService = roleService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
    {
        var result = await _roleService.GetListAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Roles retrieved successfully"));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search)
    {
        var items = new List<RoleListItemDto>();
        var page = 1;
        while (true)
        {
            var result = await _roleService.GetListAsync(new DataTableRequestDto
            {
                PageNumber = page,
                PageSize = 200,
                SearchTerm = search,
                SortColumn = "UserId",
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
        var sheet = workbook.Worksheets.Add("Roles");
        var headers = new[] { "User ID", "Full Name", "Group", "Add/Edit", "Delete", "Read Only", "Status" };
        for (var column = 0; column < headers.Length; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }

        for (var row = 0; row < items.Count; row++)
        {
            var item = items[row];
            sheet.Cell(row + 2, 1).Value = item.UserId;
            sheet.Cell(row + 2, 2).Value = item.FullName ?? string.Empty;
            sheet.Cell(row + 2, 3).Value = item.GroupName ?? item.GroupId ?? string.Empty;
            sheet.Cell(row + 2, 4).Value = item.CanAddEdit ? "Yes" : "No";
            sheet.Cell(row + 2, 5).Value = item.CanDelete ? "Yes" : "No";
            sheet.Cell(row + 2, 6).Value = item.CanReadOnly ? "Yes" : "No";
            sheet.Cell(row + 2, 7).Value = item.IsActive ? "Active" : "Inactive";
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"roles_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get(string userId)
    {
        var role = await _roleService.GetByIdAsync(userId);
        return role == null
            ? NotFound(ApiResponseFactory.NotFound<RoleEditDto>("Role not found."))
            : Ok(ApiResponseFactory.Success(role, "Role retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleEditDto dto)
        => ToActionResult(await _roleService.CreateAsync(dto));

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] RoleEditDto dto)
        => ToActionResult(await _roleService.UpdateAsync(userId, dto));

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
        => ToActionResult(await _roleService.DeleteAsync(userId));

    private IActionResult ToActionResult(BaseGeneralResponse result)
    {
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
