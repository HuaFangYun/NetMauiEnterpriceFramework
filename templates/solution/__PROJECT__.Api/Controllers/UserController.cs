using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "00")]
public class UserController : ControllerBase
{
    private readonly IUserManagementService _userService;

    public UserController(IUserManagementService userService) => _userService = userService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
    {
        var result = await _userService.GetListAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Users retrieved successfully"));
    }

    [HttpPost("list-user-flag")]
    public async Task<IActionResult> GetUserFlagList([FromBody] DataTableRequestDto request)
    {
        var result = await _userService.GetUserFlagListAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Users retrieved successfully"));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return user == null
            ? NotFound(ApiResponseFactory.NotFound<UserEditDto>("User not found."))
            : Ok(ApiResponseFactory.Success(user, "User retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserEditDto dto)
        => ToActionResult(await _userService.CreateAsync(dto));

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] UserEditDto dto)
        => ToActionResult(await _userService.UpdateAsync(userId, dto));

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
        => ToActionResult(await _userService.DeleteAsync(userId));

    [HttpPost("unlock")]
    public async Task<IActionResult> Unlock([FromBody] UserActionDto dto)
        => ToActionResult(await _userService.UnlockAsync(dto.UserId));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserActionDto dto)
        => ToActionResult(await _userService.ResetPasswordAsync(dto.UserId, dto.NewPassword ?? string.Empty));

    private IActionResult ToActionResult(BaseGeneralResponse result)
    {
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
