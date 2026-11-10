using System.Security.Claims;

using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IMenuManagementService _menuManagementService;
    private readonly IUnitOfWork _unitOfWork;

    public MenuController(
        IMenuService menuService,
        IMenuManagementService menuManagementService,
        IUnitOfWork unitOfWork)
    {
        _menuService = menuService;
        _menuManagementService = menuManagementService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserMenu()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userGroup = await _unitOfWork.UmUserGroup.GetByIdAsync(userId);
        if (userGroup == null || string.IsNullOrEmpty(userGroup.group_id))
        {
            return Ok(ApiResponseFactory.Success(new List<MenuItemDto>(), "Menus retrieved successfully"));
        }

        var menu = await _menuService.GetMenuTreeByGroupIdAsync(userGroup.group_id);
        return Ok(ApiResponseFactory.Success(menu, "Menus retrieved successfully"));
    }

    [HttpPost("list")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
    {
        var result = await _menuManagementService.GetListAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Menus retrieved successfully"));
    }

    [HttpGet("export")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Export([FromQuery] string? search)
    {
        var items = new List<MenuListItemDto>();
        var page = 1;
        while (true)
        {
            var result = await _menuManagementService.GetListAsync(new DataTableRequestDto
            {
                PageNumber = page,
                PageSize = 200,
                SearchTerm = search,
                SortColumn = "MenuUnique",
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
        var sheet = workbook.Worksheets.Add("Menus");
        var headers = new[] { "ID", "Unique", "Name", "Level", "Parent", "Order", "Status" };
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
            sheet.Cell(row + 2, 1).Value = item.Id;
            sheet.Cell(row + 2, 2).Value = item.MenuUnique;
            sheet.Cell(row + 2, 3).Value = item.MenuName ?? string.Empty;
            sheet.Cell(row + 2, 4).Value = item.ParentLevelId ?? string.Empty;
            sheet.Cell(row + 2, 5).Value = item.ParentName ?? item.MenuUniqueParent ?? string.Empty;
            sheet.Cell(row + 2, 6).Value = item.OrderSeq?.ToString() ?? string.Empty;
            sheet.Cell(row + 2, 7).Value = item.IsActive ? "Active" : "Inactive";
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"menus_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("detail/{id:long}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetDetail(long id)
    {
        var menu = await _menuManagementService.GetByIdAsync(id);
        return menu == null
            ? NotFound(ApiResponseFactory.NotFound<MenuEditDto>("Menu not found."))
            : Ok(ApiResponseFactory.Success(menu, "Menu retrieved successfully"));
    }

    [HttpPost]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Create([FromBody] MenuEditDto dto)
        => ToActionResult(await _menuManagementService.CreateAsync(dto));

    [HttpPut("{id:long}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Update(long id, [FromBody] MenuEditDto dto)
        => ToActionResult(await _menuManagementService.UpdateAsync(id, dto));

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Delete(long id)
        => ToActionResult(await _menuManagementService.DeleteAsync(id));

    [HttpGet("tree")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetTree()
        => Ok(ApiResponseFactory.Success(await _menuManagementService.GetTreeAsync(), "Menu tree retrieved successfully"));

    [HttpGet("assigned/{groupId}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetAssigned(string groupId)
        => Ok(ApiResponseFactory.Success(await _menuManagementService.GetAssignedMenuIdsAsync(groupId), "Assigned menus retrieved successfully"));

    [HttpPost("save-map")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> SaveMap([FromBody] MenuMapDto dto)
        => ToActionResult(await _menuManagementService.SaveMenuMapAsync(dto));

    private IActionResult ToActionResult(BaseGeneralResponse result)
    {
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
