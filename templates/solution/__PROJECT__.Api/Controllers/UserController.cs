using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "00")]
public class UserController : ControllerBase
{
    private readonly IUserManagementService _userService;

    public UserController(IUserManagementService userService) => _userService = userService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _userService.GetListAsync(request));

    [HttpPost("list-user-flag")]
    public async Task<IActionResult> GetUserFlagList([FromBody] DataTableRequestDto request)
        => Ok(await _userService.GetUserFlagListAsync(request));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search)
    {
        var users = new List<UserListItemDto>();
        var page = 1;

        while (true)
        {
            var result = await _userService.GetListAsync(new DataTableRequestDto
            {
                PageNumber = page,
                PageSize = 200,
                SearchTerm = search,
                SortColumn = "UserId",
                SortDirection = "asc",
            });

            users.AddRange(result.Data);
            if (users.Count >= result.TotalCount || result.Data.Count == 0)
            {
                break;
            }

            page++;
        }

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Users");
        var headers = new[]
        {
            "User Id", "Full Name", "Gender", "Email",
            "Phone Number", "Role", "Status",
        };

        for (var column = 0; column < headers.Length; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }

        for (var row = 0; row < users.Count; row++)
        {
            var user = users[row];
            var status = user.IsLocked ? "Locked" : user.IsActive ? "Active" : "Inactive";
            sheet.Cell(row + 2, 1).Value = user.UserId;
            sheet.Cell(row + 2, 2).Value = user.FullName ?? string.Empty;
            sheet.Cell(row + 2, 3).Value = user.GenderName ?? string.Empty;
            sheet.Cell(row + 2, 4).Value = user.Email ?? string.Empty;
            sheet.Cell(row + 2, 5).Value = user.PhoneNumber ?? string.Empty;
            sheet.Cell(row + 2, 6).Value = user.GroupName ?? string.Empty;
            sheet.Cell(row + 2, 7).Value = status;
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"users_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserEditDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] UserEditDto dto)
    {
        var result = await _userService.UpdateAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        var result = await _userService.DeleteAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("unlock")]
    public async Task<IActionResult> Unlock([FromBody] UserActionDto dto)
    {
        var result = await _userService.UnlockAsync(dto.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserActionDto dto)
    {
        var result = await _userService.ResetPasswordAsync(dto.UserId, dto.NewPassword ?? string.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
